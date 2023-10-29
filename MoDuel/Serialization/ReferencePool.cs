using MessagePack;
using MoDuel.Json;
using MoDuel.Tools;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MoDuel.Serialization;

/// <summary>
/// The container that objects marked with <see cref="SerializableAttribute"/> will be serialized and stored within.
/// </summary>
public class ReferencePool {

    /// <summary>
    /// The <see cref="Indexer"/> used to provide keys to references.
    /// </summary>
    private readonly Indexer Indexer = new();

    /// <summary>
    /// The lookup for the ids of objects that have already been serialized.
    /// </summary>
    private readonly Dictionary<object, int> References = new();

    /// <summary>
    /// The data for each object stored by their 
    /// </summary>
    private readonly Dictionary<int, byte[]> Data = new();

    /// <summary>
    /// The set of objects that have been Deserialized by their initial id.
    /// </summary>
    private readonly Dictionary<int, object?> RestoredReferences = new();

    // TODO SERIAL: Caches the field info for types.
    //private readonly static Dictionary<Type, FieldInfo[]> CachedFields = new();

    /// <summary>
    /// Serializes the provided <paramref name="obj"/>
    /// </summary>
    /// <param name="obj">The object to be serialized.</param>
    /// <param name="options">The options to use for any fields stored on the object.</param>
    /// <returns>The reference id that can be used to deserialize the data.</returns>
    public int Serialize(object? obj, MessagePackSerializerOptions options) {

        if (obj == null) return Indexer.UNSET_INDEX;

        // If the refernce has already been serialized retrieve that value instead.
        // TOOO: If the object has changed this will no longer work.
        if (References.TryGetValue(obj, out var refs)) {
            return refs;
        }

        // Get the refernce index the object will be provided.
        var index = Indexer.GetNext();
        // Store the object so that it won't be recusivly serialized.
        References.Add(obj, index);
        // Get the type data so we know how to serialize it.
        var type = obj.GetType();
        var fields = GetFields(type);

        // Create the container that will store the serialized data.
        SerializedObjectData objectDefenition = new(
            type.GetSimpleQualifiedName(),
            new()
        );

        // Serialize each field of the object.
        foreach (var field in fields) {
            var data = SerializeField(obj, field, options);
            // Record the data to the container.
            objectDefenition.FieldDefinitions.Add(
                field.Name,
                data
            );


        }

        // Serialize the container and store it.
        Data[index] = MessagePackSerializer.Serialize(objectDefenition, options);
        return index;
    }

    /// <summary>
    /// Serializes a <see cref="JToken"/> as they cannot be serialized normally.
    /// </summary>
    /// <param name="token">The value to serialize.</param>
    /// <param name="options">Options used to provide compression settings.</param>
    /// <returns></returns>
    public int SerializeJson(JToken token, MessagePackSerializerOptions options) {

        // Ensure the token is valid.
        if (token == null) return Indexer.UNSET_INDEX;

        // check to see if the token was already serialized.
        if (!References.TryGetValue(token, out var refs)) {
            return refs;
        }

        // Serialize non rooted tokens as a proxy.
        if (!token.IsRoot()) {
            var proxy = token.ToProxy();
            proxy.ForceToToken = true;
            return Serialize(proxy, options);
        }

        // Create the container that will store the serialized data.
        SerializedObjectData objectDefenition = new(
            token.GetType().GetSimpleQualifiedName(),
            new()
        );

        // Serialize the token or its relative information.
        objectDefenition.FieldDefinitions.Add("Data", MessagePackSerializer.Serialize(new JsonData(token)));

        // Get the refernce index the object will be provided.
        var index = Indexer.GetNext();
        // Store the object
        References.Add(token, index);

        // Serialize the container and store it.
        Data[index] = MessagePackSerializer.Serialize(objectDefenition, options);
        return index;

    }

    /// <summary>
    /// Deserializes a json value that was seralized using <see cref="SerializeJson(JToken, MessagePackSerializerOptions)"/>.
    /// </summary>
    /// <param name="jsonData">The data by the serialize method.</param>
    /// <returns>The <see cref="JToken"/> or equivalant that was deserialized.</returns>
    public static object DeserializeJson(SerializedObjectData jsonData) {
        // Get the 'field' that was created.
        if (jsonData.FieldDefinitions.TryGetValue("Data", out var data)) {
            // Get the token informatiom.
            var info = MessagePackSerializer.Deserialize<JsonData>(data);
            return info.Deserialize();
        }
        return DeadToken.Instance;
    }

    /// <summary>
    /// Get the fields for the provided type that should be serialized.
    /// </summary>
    private static IEnumerable<FieldInfo> GetFields(Type t) {
        IEnumerable<FieldInfo> fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        fields = fields.Where((f) => f.GetCustomAttribute<IgnoreMemberAttribute>() == null);
        return fields;
    }

    /// <summary>
    /// Serialzies the field on the <paramref name="obj"/> using its <paramref name="fieldInfo"/>.
    /// </summary>
    /// <param name="obj">The object that has the field.</param>
    /// <param name="fieldInfo">THe field info to find the field.</param>
    /// <param name="options">The serialization settings.</param>
    /// <returns>The data after the serialization process.</returns>
    private static byte[] SerializeField(object obj, FieldInfo fieldInfo, MessagePackSerializerOptions options) {
        var field = fieldInfo.GetValue(obj);
        return MessagePackSerializer.Serialize(fieldInfo.FieldType, field, options);
    }

    /// <summary>
    /// Retreives the serialized data currently stored.
    /// </summary>
    public ReadOnlyDictionary<int, byte[]> GetData() => Data.AsReadOnly();

    /// <summary>
    /// Reserialize a dictionary of data that was in the same format as <see cref="Data"/>.
    /// </summary>
    /// <param name="data">The data to be deserialized.</param>
    /// <param name="options">The message pack deserialization settings.</param>
    public void DeserializeAll(IDictionary<int, byte[]> data, MessagePackSerializerOptions options) {

        // Unpack the serialization data.
        // The data containers are deserialized and the type of the original data is also extracted.
        var definitions = data.Select((pair) => {
            SerializedObjectData definition = MessagePackSerializer.Deserialize<SerializedObjectData>(pair.Value, options);
            var type = Type.GetType(definition.TypeName, false);
            return new Tuple<int, SerializedObjectData, Type?>(pair.Key, definition, type);
        });

        // Create a new object of the expected type but don't populate it yet.
        // This is to ensure circular references are loaded successfully.
        foreach (var definition in definitions) {
            var type = definition.Item3;
            // Can't deserialize items with an unknown type.
            if (type == null) {
                RestoredReferences[definition.Item1] = null;
                continue;
            }
            // Have to handle json values specifically.
            if (type.IsAssignableFrom(typeof(JToken))) {
                RestoredReferences[definition.Item1] = DeserializeJson(definition.Item2);
                continue;
            }
            // Create the object in a blank state.
            RestoredReferences[definition.Item1] = RuntimeHelpers.GetUninitializedObject(type);
        }

        // Deserialize each field value.
        foreach (var definition in definitions) {

            var type = definition.Item3;
            var obj = RestoredReferences[definition.Item1];

            // Can't deserialize items with an unknown type.
            if (type == null) {
                continue;
            }

            // Json values have be handled differently.
            if (type.IsAssignableTo(typeof(JToken))) {
                continue;
            }

            // Get all the fields that could have a value stored in the data.
            var fields = GetFields(type);

            // Set the value of a field to the value deserialized from the byte data.
            foreach (var field in fields) {
                if (definition.Item2.FieldDefinitions.TryGetValue(field.Name, out var fieldDefinition)) {
                    var value = MessagePackSerializer.Deserialize(field.FieldType, fieldDefinition, options);
                    field.SetValue(obj, value);
                }
            }

            // Force some json proxies to json tokens.
            // This should be a safe as it does not rely on anything but JTokens which will never rely on it.
            if (obj is JsonProxy proxy && proxy.ForceToToken) {
                RestoredReferences[definition.Item1] = proxy.Token;
            }
        }

    }

    /// <summary>
    /// Returns the object that was deserialized with the given refernce id.
    /// </summary>
    /// <param name="key">The refernce the object was both serialized and derserialized with.</param>
    /// <returns>The ovjet if it exists; otherwise null.</returns>
    public object? ReadReferncedObject(int key) {
        if (RestoredReferences.TryGetValue(key, out var refs)) {
            return refs;
        }
        return null;
    }

}
