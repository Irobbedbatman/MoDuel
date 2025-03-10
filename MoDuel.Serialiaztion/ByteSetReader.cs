namespace MoDuel.Serialization;

/// <summary>
/// A wrapper around a set of bytes to assist in reading values.
/// </summary>
public class ByteSetReader {

    /// <summary>
    /// The data the reader will parse.
    /// </summary>
    public readonly byte[] Data;

    /// <summary>
    /// Pointer that points to the next byte to read.
    /// </summary>
    private int _pointer;

    /// <summary>
    /// Construct a new reader which will assist in handling the provided byte data.
    /// </summary>
    public ByteSetReader(byte[] data) => Data = data;

    /// <summary>
    /// Construct a new reader which will assist in handling bytes written to a writer.
    /// </summary>
    public ByteSetReader(ByteSetWriter writer) => Data = writer.Data;

    /// <summary>
    /// Move the pointer by the increment after returning the current pointer position.
    /// </summary>
    private int PreMove(int increment) => (_pointer += increment) - increment;

    /// <summary>
    /// Get the current pointer position.
    /// </summary>
    public int GetCurrentPosition() => _pointer;

    /// <summary>
    /// Set the position of the pointer.
    /// </summary>
    public void SetPosition(int position) => _pointer = position;

    /// <summary>
    /// Reset the pointer's position back to 0.
    /// </summary>
    public void Reset() => _pointer = 0;

    /// <summary>
    /// Retrieve the next bool from the data.
    /// </summary>
    public bool GetBool() => PrimitiveSerialization.DeserializeBool(Data, _pointer++);
    /// <summary>
    /// Retrieve the next byte from the data.
    /// </summary>
    public byte GetByte() => Data[_pointer++];
    /// <summary>
    /// Retrieve the next sbyte from the data.
    /// </summary>
    public sbyte GetSByte() => (sbyte)(Data[_pointer++] - 128);
    /// <summary>
    /// Retrieve the next short from the data.
    /// </summary>
    public short GetShort() => PrimitiveSerialization.DeserializeShort(Data, PreMove(2));
    /// <summary>
    /// Retrieve the next ushort from the data.
    /// </summary>
    public ushort GetUShort() => PrimitiveSerialization.DeserializeUShort(Data, PreMove(2));
    /// <summary>
    /// Retrieve the next int from the data.
    /// </summary>
    public int GetInt() => PrimitiveSerialization.DeserializeInt(Data, PreMove(4));
    /// <summary>
    /// Retrieve the next uint from the data.
    /// </summary>
    public uint GetUInt() => PrimitiveSerialization.DeserializeUInt(Data, PreMove(4));
    /// <summary>
    /// Retrieve the next long from the data.
    /// </summary>
    public long GetLong() => PrimitiveSerialization.DeserializeLong(Data, PreMove(8));
    /// <summary>
    /// Retrieve the next ulong from the data.
    /// </summary>
    public ulong GetULong() => PrimitiveSerialization.DeserializeULong(Data, PreMove(8));
    /// <summary>
    /// Retrieve the next char from the data.
    /// </summary>
    public char GetChar() => PrimitiveSerialization.DeserializeChar(Data, PreMove(2));
    /// <summary>
    /// Retrieve the next float from the data.
    /// </summary>
    public float GetFloat() => PrimitiveSerialization.DeserializeFloat(Data, PreMove(4));
    /// <summary>
    /// Retrieve the next double from the data.
    /// </summary>
    public double GetDouble() => PrimitiveSerialization.DeserializeDouble(Data, PreMove(8));
    /// <summary>
    /// Retrieve the next datetime from the data.
    /// </summary>
    public DateTime GetDateTime() => DateTime.FromBinary(GetLong());
    /// <summary>
    /// Retrieve the next byte span from the data.
    /// </summary>
    public Span<byte> GetByteSpan() => ArraySerialization.DeserializeSpan<byte>(this);
    /// <summary>
    /// Retrieve the next byte array from the data.
    /// </summary>
    public byte[]? GetByteArray() => ArraySerialization.Deserialize<byte>(this);
    /// <summary>
    /// Retrieve the next typed array from the data.
    /// </summary>
    public T[]? GetArray<T>() => ArraySerialization.Deserialize<T>(this);
    /// <summary>
    /// Retrieve the next typed span from the data.
    /// </summary>
    public Span<T> GetSpan<T>() => ArraySerialization.DeserializeSpan<T>(this);
    /// <summary>
    /// Retrieve the next typed readonly span from the data.
    /// </summary>
    public ReadOnlySpan<T> GetReadOnlySpan<T>() => ArraySerialization.DeserializeReadOnlySpan<T>(this);
    /// <summary>
    /// Retrieve the next string from the data.
    /// </summary>
    public string? GetString() => ArraySerialization.DeserializeString(this);
    /// <summary>
    /// Retrieve the next object from the data.
    /// </summary>
    public object? GetObject() => ObjectSerialization.Deserialize(this);

    /// <summary>
    /// Retrieve the next explicit <see cref="IByteSetSerializable"/> from the data.
    /// </summary>
    public T GetByteSetSerializable<T>(Func<T> constructor) where T : IByteSetSerializable {
        var result = constructor();
        result.Deserialize(this);
        return result;
    }

    /// <summary>
    /// Retrieve the next bool from the data without incrementing the pointer..
    /// </summary>
    public bool PeekBool() => PrimitiveSerialization.DeserializeBool(Data, _pointer);
    /// <summary>
    /// Retrieve the next byte from the data without incrementing the pointer..
    /// </summary>
    public byte PeekByte() => Data[_pointer];
    /// <summary>
    /// Retrieve the next sbyte from the data without incrementing the pointer..
    /// </summary>
    public sbyte PeekSByte() => (sbyte)(Data[_pointer] - 128);
    /// <summary>
    /// Retrieve the next char from the data without incrementing the pointer..
    /// </summary>
    public char PeekChar() => PrimitiveSerialization.DeserializeChar(Data, _pointer);
    /// <summary>
    /// Retrieve the next short from the data without incrementing the pointer..
    /// </summary>
    public short PeekShort() => PrimitiveSerialization.DeserializeShort(Data, _pointer);
    /// <summary>
    /// Retrieve the next ushort from the data without incrementing the pointer..
    /// </summary>
    public ushort PeekUShort() => PrimitiveSerialization.DeserializeUShort(Data, _pointer);
    /// <summary>
    /// Retrieve the next int from the data without incrementing the pointer..
    /// </summary>
    public int PeekInt() => PrimitiveSerialization.DeserializeInt(Data, _pointer);
    /// <summary>
    /// Retrieve the next uint from the data without incrementing the pointer..
    /// </summary>
    public uint PeekUInt() => PrimitiveSerialization.DeserializeUInt(Data, _pointer);
    /// <summary>
    /// Retrieve the next long from the data without incrementing the pointer..
    /// </summary>
    public long PeekLong() => PrimitiveSerialization.DeserializeLong(Data, _pointer);
    /// <summary>
    /// Retrieve the next ulong from the data without incrementing the pointer..
    /// </summary>
    public ulong PeekULong() => PrimitiveSerialization.DeserializeULong(Data, _pointer);
    /// <summary>
    /// Retrieve the next float from the data without incrementing the pointer..
    /// </summary>
    public float PeekFloat() => PrimitiveSerialization.DeserializeFloat(Data, _pointer);
    /// <summary>
    /// Retrieve the next double from the data without incrementing the pointer..
    /// </summary>
    public double PeekDouble() => PrimitiveSerialization.DeserializeDouble(Data, _pointer);
    /// <summary>
    /// Retrieve the next datetime from the data without incrementing the pointer..
    /// </summary>
    public DateTime PeekDateTime() => DateTime.FromBinary(PeekLong());
    /// <summary>
    /// Retrieve the next span of bytes with the provided <paramref name="length"/> from the data without incrementing the pointer..
    /// </summary>
    public Span<byte> PeekSpan(int length) => Data.AsSpan(_pointer, length);

}
