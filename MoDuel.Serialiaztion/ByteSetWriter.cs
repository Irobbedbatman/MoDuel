namespace MoDuel.Serialization;

/// <summary>
/// A wrapper around a byte to allow for easier conversion of multiple values.
/// </summary>
public class ByteSetWriter {

    /// <summary>
    /// The factor to resize the array to fit new elements.
    /// </summary>
    private const int ResizeFactor = 2;

    /// <summary>
    /// The current position in the bytes to write to.
    /// </summary>
    private int _pointer = 0;

    /// <summary>
    /// The data being written to.
    /// </summary>
    private byte[] data;

    /// <summary>
    /// The data being written to.
    /// </summary>
    public byte[] Data => data;

    /// <summary>
    /// Indicates that the data array will be resized if an inserted element would not fit otherwise.
    /// </summary>
    public bool AutoResize;

    /// <summary>
    /// Create a new instance of the writer.
    /// </summary>
    /// <param name="length">The initial length of the bytes.</param>
    /// <param name="autoResize">Whether the data array should be resized to fit new elements.</param>
    public ByteSetWriter(int length = 0, bool autoResize = true) {
        data = new byte[length];
        AutoResize = autoResize;
    }

    /// <summary>
    /// Attempt to resize the array to the new size.
    /// </summary>
    /// <param name="newSize"></param>
    /// <exception cref="Exception"></exception>
    private void Resize(int newSize) {
        if (AutoResize) {
            Array.Resize(ref data, Math.Max(newSize, data.Length * ResizeFactor));
        }
        else {
            throw new Exception("Cannot resize data buffer to fit in written element.");
        }
    }

    /// <summary>
    /// Get the next span of <paramref name="length"/> bytes.
    /// </summary>
    private Span<byte> GetNextSpan(int length) {
        if (data.Length < _pointer + length) {
            Resize(data.Length + length);
        }
        return data.AsSpan((_pointer += length) - length);
    }

    /// <summary>
    /// Reset the pointer and move the data to the start.
    /// </summary>
    public void Shuffle() {
        Reset();
        Put(Data);
    }

    /// <summary>
    /// Reset the pointer back to 0.
    /// </summary>
    public void Reset() => _pointer = 0;

    /// <summary>
    /// Insert a byte as the next element.
    /// </summary>
    public void Put(byte b) => GetNextSpan(1)[0] = b;
    /// <summary>
    /// Insert a sbyte as the next element.
    /// </summary>
    public void Put(sbyte b) => GetNextSpan(1)[0] = (byte)(b + 128);
    /// <summary>
    /// Insert a bool as the next element.
    /// </summary>
    public void Put(bool b) => PrimitiveSerialization.Serialize(GetNextSpan(1), b);
    /// <summary>
    /// Insert a short as the next element.
    /// </summary>
    public void Put(short s) => PrimitiveSerialization.Serialize(GetNextSpan(2), s);
    /// <summary>
    /// Insert a ushort as the next element.
    /// </summary>
    public void Put(ushort s) => PrimitiveSerialization.Serialize(GetNextSpan(2), s);
    /// <summary>
    /// Insert a int as the next element.
    /// </summary>
    public void Put(int i) => PrimitiveSerialization.Serialize(GetNextSpan(4), i);
    /// <summary>
    /// Insert a uint as the next element.
    /// </summary>
    public void Put(uint i) => PrimitiveSerialization.Serialize(GetNextSpan(4), i);
    /// <summary>
    /// Insert a long as the next element.
    /// </summary>
    public void Put(long l) => PrimitiveSerialization.Serialize(GetNextSpan(8), l);
    /// <summary>
    /// Insert a ulong as the next element.
    /// </summary>
    public void Put(ulong l) => PrimitiveSerialization.Serialize(GetNextSpan(8), l);
    /// <summary>
    /// Insert a char as the next element.
    /// </summary>
    public void Put(char c) => PrimitiveSerialization.Serialize(GetNextSpan(2), c);
    /// <summary>
    /// Insert a float as the next element.
    /// </summary>
    public void Put(float f) => PrimitiveSerialization.Serialize(GetNextSpan(4), f);
    /// <summary>
    /// Insert a double as the next element.
    /// </summary>
    public void Put(double d) => PrimitiveSerialization.Serialize(GetNextSpan(8), d);
    /// <summary>
    /// Insert a date time as the next element.
    /// </summary>
    public void Put(DateTime dt) => PrimitiveSerialization.Serialize(GetNextSpan(8), dt.ToBinary());
    /// <summary>
    /// Insert a span of bytes as the next element.
    /// </summary>
    public void Put(Span<byte> bs) => bs.CopyTo(GetNextSpan(bs.Length));
    /// <summary>
    /// Insert an array of bytes as the next element.
    /// </summary>
    public void Put(byte[] ba) => ba.CopyTo(GetNextSpan(ba.Length));
    /// <summary>
    /// Insert an array as the next element.
    /// </summary>
    public void Put(Array array) => ArraySerialization.Serialize(this, array);
    /// <summary>
    /// Insert a typed span as the next element.
    /// </summary>
    public void Put<T>(Span<T> span) => ArraySerialization.Serialize(this, span);
    /// <summary>
    /// Insert a typed read only span as the next element.
    /// </summary>
    public void Put<T>(ReadOnlySpan<T> span) => ArraySerialization.Serialize(this, span);
    /// <summary>
    /// Insert a string as the next element.
    /// </summary>
    public void Put(string? str) => ArraySerialization.Serialize(this, str);
    /// <summary>
    /// Insert another writer's data as the next element.
    /// </summary>
    public void Put(ByteSetWriter serializer) => Put(serializer.data);
    /// <summary>
    /// Insert an object as the next element.
    /// </summary>
    public void PutObject(object? obj) => ObjectSerialization.Serialize(this, obj);
    /// <summary>
    /// Insert a fixed type code as the next element.
    /// </summary>
    public void Put(FixedTypeCodes code) => Put((short)code);
    /// <summary>
    /// Insert a <see cref="IByteSetSerializable"/> as the next element.
    /// </summary>
    public void Put(IByteSetSerializable byteSetSerializable) => byteSetSerializable.Serialize(this);

}
