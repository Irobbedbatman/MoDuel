using MoDuel.Serialization;

namespace MoDuel.Tcp.Packets;

/// <summary>
/// The password used for authentication after a connection.
/// </summary>
public class AuthenticationPacket : IByteSetSerializable {

    /// <summary>
    /// The password sent.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Additional data used to authenticate.
    /// </summary>
    public byte[] UserData { get; set; } = [];

    public void Deserialize(ByteSetReader reader) {
        Password = reader.GetString() ?? "";
        UserData = reader.GetByteArray() ?? [];
    }

    public void Serialize(ByteSetWriter writer) {
        writer.Put(Password);
        writer.Put(UserData);
    }
}
