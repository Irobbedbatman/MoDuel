namespace MoDuel.Serialization;

/// <summary>
/// Serialization for objects of an unknown type.
/// </summary>
public static class ObjectSerialization {

    /// <summary>
    /// Write the provided obj to the writer.
    /// </summary>
    public static void Serialize(ByteSetWriter writer, object? obj) {
        switch (obj) {
            case null:
                writer.Put(FixedTypeCodes.Null);
                break;
            case bool bo:
                writer.Put(FixedTypeCodes.Bool);
                writer.Put(bo);
                break;
            case byte b:
                writer.Put(FixedTypeCodes.Byte);
                writer.Put(b);
                break;
            case sbyte sb:
                writer.Put(FixedTypeCodes.SByte);
                writer.Put(sb);
                break;
            case int i:
                writer.Put(FixedTypeCodes.Int);
                writer.Put(i);
                break;
            case uint ui:
                writer.Put(FixedTypeCodes.UInt);
                writer.Put(ui);
                break;
            case short s:
                writer.Put(FixedTypeCodes.Short);
                writer.Put(s);
                break;
            case ushort us:
                writer.Put(FixedTypeCodes.UShort);
                writer.Put(us);
                break;
            case long l:
                writer.Put(FixedTypeCodes.Long);
                writer.Put(l);
                break;
            case ulong ul:
                writer.Put(FixedTypeCodes.ULong);
                writer.Put(ul);
                break;
            case char c:
                writer.Put(FixedTypeCodes.Char);
                writer.Put(c);
                break;
            case float f:
                writer.Put(FixedTypeCodes.Float);
                writer.Put(f);
                break;
            case double d:
                writer.Put(FixedTypeCodes.Double);
                writer.Put(d);
                break;
            case DateTime dt:
                writer.Put(FixedTypeCodes.DateTime);
                writer.Put(dt);
                break;
            case string s:
                writer.Put(FixedTypeCodes.String);
                writer.Put(s);
                break;
            case Array array:
                writer.Put(FixedTypeCodes.Array);
                ArraySerialization.Serialize(writer, array, true);
                break;
            default:
                TypeRegister.WriteObject(writer, obj);
                break;
        }
    }


    /// <summary>
    /// Read the next object stored in the <paramref name="reader"/>.
    /// </summary>
    public static object? Deserialize(ByteSetReader reader) {
        short type = reader.GetShort();
        if (type > 0) {
            return TypeRegister.ReadObject(reader, type);
        }
        else {
            FixedTypeCodes code = (FixedTypeCodes)type;
            return code switch {
                FixedTypeCodes.Null => null,
                FixedTypeCodes.Byte => reader.GetByte(),
                FixedTypeCodes.SByte => reader.GetSByte(),
                FixedTypeCodes.Short => reader.GetShort(),
                FixedTypeCodes.UShort => reader.GetUShort(),
                FixedTypeCodes.Int => reader.GetInt(),
                FixedTypeCodes.UInt => reader.GetUInt(),
                FixedTypeCodes.Long => reader.GetLong(),
                FixedTypeCodes.ULong => reader.GetULong(),
                FixedTypeCodes.Char => reader.GetChar(),
                FixedTypeCodes.Float => reader.GetFloat(),
                FixedTypeCodes.Double => reader.GetDouble(),
                FixedTypeCodes.DateTime => reader.GetDateTime(),
                FixedTypeCodes.String => reader.GetString(),
                FixedTypeCodes.Array => ArraySerialization.DeserializeEmbedded(reader),
                _ => TypeRegister.ReadObject(reader),
            };
        }

    }

}
