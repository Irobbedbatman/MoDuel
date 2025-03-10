namespace MoDuel.Serialization;

/// <summary>
/// A class that can be serialized directly into a byte set.
/// </summary>
public interface IByteSetSerializable {

    /// <summary>
    /// Write the object in the writer.
    /// </summary>
    public void Serialize(ByteSetWriter writer);

    /// <summary>
    /// Read the object from the reader.
    /// </summary>
    public void Deserialize(ByteSetReader reader);

}
