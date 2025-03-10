using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace MoDuel.Serialization;

/// <summary>
/// Methods to serialize and deserialize primitive values.
/// </summary>
public static unsafe class PrimitiveSerialization {

    private const byte b0 = 0;
    private const byte b1 = 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(bool b) => [b ? b1 : b0];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, bool b) => bytes[0] = b ? b1 : b0;
    public static byte[] Serialize(short s) => BitConverter.GetBytes(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, short s) => BinaryPrimitives.WriteInt16LittleEndian(bytes, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(ushort s) => BitConverter.GetBytes(s);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, ushort s) => BinaryPrimitives.WriteUInt16LittleEndian(bytes, s);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(int i) => BitConverter.GetBytes(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, int i) => BinaryPrimitives.WriteInt32LittleEndian(bytes, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(uint i) => BitConverter.GetBytes(i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, uint i) => BinaryPrimitives.WriteUInt32LittleEndian(bytes, i);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(long l) => BitConverter.GetBytes(l);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, long l) => BinaryPrimitives.WriteInt64LittleEndian(bytes, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(ulong l) => BitConverter.GetBytes(l);
    public static void Serialize(Span<byte> bytes, ulong l) => BinaryPrimitives.WriteUInt64LittleEndian(bytes, l);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(char c) => BitConverter.GetBytes(c);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, char c) {
        if (BitConverter.IsLittleEndian) {
            bytes[0] = (byte)(c & 0xFF);
            bytes[1] = (byte)(c >> 8);

        }
        else {
            bytes[0] = (byte)(c >> 8);
            bytes[1] = (byte)(c & 0xFF);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(float f) => BitConverter.GetBytes(f);
    public static void Serialize(Span<byte> bytes, float f) => BinaryPrimitives.WriteSingleLittleEndian(bytes, f);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(double d) => BitConverter.GetBytes(d);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Span<byte> bytes, double d) => BinaryPrimitives.WriteDoubleLittleEndian(bytes, d);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DeserializeBool(byte[] bytes, int startIndex = 0) => bytes[startIndex] == b1;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char DeserializeChar(byte[] bytes, int startIndex = 0) => BitConverter.ToChar(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short DeserializeShort(byte[] bytes, int startIndex = 0) => BitConverter.ToInt16(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort DeserializeUShort(byte[] bytes, int startIndex = 0) => BitConverter.ToUInt16(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DeserializeInt(byte[] bytes, int startIndex = 0) => BitConverter.ToInt32(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint DeserializeUInt(byte[] bytes, int startIndex = 0) => BitConverter.ToUInt32(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DeserializeLong(byte[] bytes, int startIndex = 0) => BitConverter.ToInt64(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong DeserializeULong(byte[] bytes, int startIndex = 0) => BitConverter.ToUInt64(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DeserializeFloat(byte[] bytes, int startIndex = 0) => BitConverter.ToSingle(bytes, startIndex);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DeserializeDouble(byte[] bytes, int startIndex = 0) => BitConverter.ToDouble(bytes, startIndex);


}
