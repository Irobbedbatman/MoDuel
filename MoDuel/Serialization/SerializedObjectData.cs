using MessagePack;
namespace MoDuel.Serialization;

/// <summary>
/// The type and field data stored for the object so that it easier to deserialize them.
/// </summary>
[MessagePackObject]
public readonly struct SerializedObjectData {

    /// <summary>
    /// The <see cref="Type"/> the object data is as it's searchable name.
    /// </summary>
    [Key(0)]
    public readonly string TypeName;
    /// <summary>
    /// Each field of the object as its field name and the value of that field stored as byte data.
    /// </summary>
    [Key(1)]
    public readonly Dictionary<string, byte[]> FieldDefinitions;

    public SerializedObjectData(string typeName, Dictionary<string, byte[]> fieldDefinitions) {
        TypeName = typeName;
        FieldDefinitions = fieldDefinitions;
    }
}
