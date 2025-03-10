namespace MoDuel.Serialization;

/// <summary>
/// Methods to serialize and deserialize arrays, spans and strings.
/// </summary>
public static class ArraySerialization {

    /// <summary>
    /// Serialize a span as an array.
    /// </summary>
    public static void Serialize<T>(ByteSetWriter writer, ReadOnlySpan<T> span) => Serialize(writer, span.ToArray());

    /// <summary>
    /// Serialize a span as an array.
    /// </summary>
    public static void Serialize<T>(ByteSetWriter writer, Span<T> span) => Serialize(writer, span.ToArray());

    /// <summary>
    /// Deserialize a span treated as an array in the provided bytes.
    /// </summary>
    public static ReadOnlySpan<T> DeserializeReadOnlySpan<T>(ByteSetReader reader) => new(Deserialize<T>(reader));

    /// <summary>
    /// Deserialize a span treated as an array in the provided bytes.
    /// </summary>
    public static Span<T> DeserializeSpan<T>(ByteSetReader reader) => new(Deserialize<T>(reader));

    /// <summary>
    /// Serialize a string as an array.
    /// </summary>
    public static void Serialize(ByteSetWriter writer, string? s) => Serialize(writer, s?.ToArray());

    /// <summary>
    /// Deserialize a string treated as an array in the provided bytes.
    /// </summary>
    public static string? DeserializeString(ByteSetReader reader) {
        var result = Deserialize<char>(reader);
        if (result == null) return null;
        return new(result);
    }

    /// <summary>
    /// Serialize an array into bytes. 
    /// </summary>
    /// <param name="embedType">When true, the type of the array is also serialized so that the deserialize can work on unknown types.</param>
    public static void Serialize(ByteSetWriter writer, Array? array, bool embedType = false) {
        Type? t = array?.GetType().GetElementType();
        var systemTypeCode = Type.GetTypeCode(t);
        if (embedType) {
            writer.Put((int)systemTypeCode);
        }
        // Check for null.
        if (array == null) {
            writer.Put(-1);
            return;
        }
        switch (systemTypeCode) {
            case TypeCode.Boolean:
                SerializationHelper<bool>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Char:
                SerializationHelper<char>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.SByte:
                SerializationHelper<sbyte>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Byte:
                SerializationHelper<byte>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Int16:
                SerializationHelper<short>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.UInt16:
                SerializationHelper<ushort>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Int32:
                SerializationHelper<int>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.UInt32:
                SerializationHelper<uint>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Int64:
                SerializationHelper<long>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.UInt64:
                SerializationHelper<ulong>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Single:
                SerializationHelper<float>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Double:
                SerializationHelper<double>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.DateTime:
                SerializationHelper<DateTime>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.String:
                SerializationHelper<string>(writer, array, static (writer, value) => writer.Put(value));
                break;
            case TypeCode.Object:
                SerializationHelper<object>(writer, array, static (writer, value) => writer.PutObject(value));
                break;
            case TypeCode.Empty:
            case TypeCode.DBNull:
            case TypeCode.Decimal:
                throw new NotSupportedException($"Cannot serialize the following type of array: {systemTypeCode}");

        }

    }

    /// <summary>
    /// Helper to serialize the array as an explicit type and the length.
    /// </summary>
    private static void SerializationHelper<T>(ByteSetWriter writer, Array array, Action<ByteSetWriter, T> serializer) {
        var length = array.Length;
        writer.Put(length);
        foreach (var item in array) {
            serializer.Invoke(writer, (T)item);
        }
    }

    /// <summary>
    /// Deserialize an array from bytes.
    /// </summary>
    public static T[]? Deserialize<T>(ByteSetReader reader) {
        TypeCode systemTypeCode = Type.GetTypeCode(typeof(T));
        var testLength = reader.PeekInt();
        if (testLength == -1) {
            reader.GetInt();
            return null;
        }
        return DeserializeInner(reader, systemTypeCode).Cast<T>().ToArray();
    }

    /// <summary>
    /// Deserialize an array from bytes that also contains the embedded type information.
    /// </summary>
    public static Array? DeserializeEmbedded(ByteSetReader reader) {
        var systemTypeCode = (TypeCode)reader.GetInt();
        var testLength = reader.PeekInt();
        if (testLength == -1) {
            reader.GetInt();
            return null;
        }
        return DeserializeInner(reader, systemTypeCode);
    }

    /// <summary>
    /// The shared deserializer once the system type code is known.
    /// </summary>
    private static Array DeserializeInner(ByteSetReader reader, TypeCode code) {
        switch (code) {
            case TypeCode.Boolean:
                return DeserializeHelper(reader, static (reader) => reader.GetBool());
            case TypeCode.Char:
                return DeserializeHelper(reader, static (reader) => reader.GetChar());
            case TypeCode.SByte:
                return DeserializeHelper(reader, static (reader) => reader.GetSByte());
            case TypeCode.Byte:
                return DeserializeHelper(reader, static (reader) => reader.GetByte());
            case TypeCode.Int16:
                return DeserializeHelper(reader, static (reader) => reader.GetShort());
            case TypeCode.UInt16:
                return DeserializeHelper(reader, static (reader) => reader.GetUShort());
            case TypeCode.Int32:
                return DeserializeHelper(reader, static (reader) => reader.GetInt());
            case TypeCode.UInt32:
                return DeserializeHelper(reader, static (reader) => reader.GetUInt());
            case TypeCode.Int64:
                return DeserializeHelper(reader, static (reader) => reader.GetLong());
            case TypeCode.UInt64:
                return DeserializeHelper(reader, static (reader) => reader.GetULong());
            case TypeCode.Single:
                return DeserializeHelper(reader, static (reader) => reader.GetFloat());
            case TypeCode.Double:
                return DeserializeHelper(reader, static (reader) => reader.GetDouble());
            case TypeCode.DateTime:
                return DeserializeHelper(reader, static (reader) => reader.GetDateTime());
            case TypeCode.String:
                return DeserializeHelper(reader, static (reader) => reader.GetString());
            case TypeCode.Object:
                return DeserializeHelper(reader, static (reader) => reader.GetObject());
            case TypeCode.Decimal:
            case TypeCode.Empty:
            case TypeCode.DBNull:
                break;
        }
        return Array.Empty<object>();
    }

    /// <summary>
    /// Helper to deserialize an array as an explicit type from the reader.
    /// </summary>
    private static T?[] DeserializeHelper<T>(ByteSetReader reader, Func<ByteSetReader, T?> deserializer) {
        int length = reader.GetInt();
        var result = new T?[length];
        for (int i = 0; i < length; i++) {
            result[i] = deserializer(reader);
        }
        return result;
    }
}
