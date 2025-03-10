namespace MoDuel.Serialization;

/// <summary>
/// The type codes for fixed types such as primitives.
/// </summary>
public enum FixedTypeCodes : short {

    Null = -1,
    Bool = -2,
    Byte = -3,
    SByte = -4,
    Short = -5,
    UShort = -6,
    Int = -7,
    UInt = -8,
    Long = -9,
    ULong = -10,
    Char = -11,
    Float = -12,
    Double = -13,
    DateTime = -14,
    String = -15,
    ByteSetSerializable = -16,
    Object = -17,
    Array = -18,
}
