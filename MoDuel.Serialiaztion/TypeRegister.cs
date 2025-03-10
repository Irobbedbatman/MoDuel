namespace MoDuel.Serialization;

/// <summary>
/// The type register to handle objects by provide a type code.
/// </summary>
public static class TypeRegister {

    /// <summary>
    /// The internal handler to manage the serialization and deserialization events.
    /// </summary>
    private class TypeTarget {

        /// <summary>
        /// The action to serialize th object to the writer.
        /// </summary>
        public readonly Action<ByteSetWriter, object?> Serialize;
        /// <summary>
        /// The action to retrieve the object from the reader.
        /// </summary>
        public readonly Func<ByteSetReader, object?> Deserialize;

        public TypeTarget(Action<ByteSetWriter, object?> serialize, Func<ByteSetReader, object?> deserialize) {
            Serialize = serialize;
            Deserialize = deserialize;
        }
    }

    /// <summary>
    /// The recorded type targets by their type code.
    /// </summary>
    private static readonly Dictionary<short, TypeTarget> Records = [];
    /// <summary>
    /// The type code look up.
    /// </summary>
    private static readonly Dictionary<Type, short> TypeCodeLookup = [];

    /// <summary>
    /// Registers a type.
    /// </summary>
    /// <param name="typeCode">The unique code for the type to be referenced by.</param>
    /// <param name="serialize">The action to serialize objects of the type.</param>
    /// <param name="deserialize">The action to deserialize objects of the type.</param>
    public static void Register<T>(short typeCode, Action<ByteSetWriter, T?> serialize, Func<ByteSetReader, T?> deserialize) {
        // Only allow one unique type code.
        if (Records.ContainsKey(typeCode)) {
            return;
        }
        var target = new TypeTarget((w, d) => serialize(w, (T?)d), (r) => deserialize(r));
        Records[typeCode] = target;
        TypeCodeLookup[typeof(T)] = typeCode;
    }

    /// <summary>
    /// Register a <see cref="IByteSetSerializable"/> type.
    /// </summary>
    /// <param name="typeCode">The unique code for the type to be referenced by.</param>
    /// <param name="constructor">The constructor of the type, used to create a blank object before deserialization.</param>
    public static void Register<T>(short typeCode, Func<T> constructor) where T : IByteSetSerializable {
        // Only allow one unique type code.
        if (Records.ContainsKey(typeCode)) return;
        // Wrap the target into the interfaced serialization methods.
        var target = new TypeTarget((w, s) => ((T?)s)?.Serialize(w),
            (r) => {
                var result = constructor();
                result.Deserialize(r);
                return result;
                }
            );
        Records[typeCode] = target;
        TypeCodeLookup[typeof(T)] = typeCode;
    }

    /// <summary>
    /// Write the provided <paramref name="obj"/> to the <paramref name="writer"/>.
    /// </summary>
    public static void WriteObject(ByteSetWriter writer, object obj) {
        if (!TypeCodeLookup.TryGetValue(obj.GetType(), out var typeCode)) return;
        if (!Records.TryGetValue(typeCode, out var target)) return;
        writer.Put(typeCode);
        target.Serialize(writer, obj);
    }

    /// <summary>
    /// Read an object with the provided type code from the <paramref name="reader"/>.
    /// </summary>
    /// <param name="typeCode">The unique type code of the data stored.</param>
    public static object? ReadObject(ByteSetReader reader, short typeCode = -1) {
        if (typeCode < 0) {
            typeCode = reader.GetShort();
        }
        if (!Records.TryGetValue(typeCode, out var target)) return default;
        return target.Deserialize(reader);
    }

}
