using MoDuel.Serialization;

namespace MoDuel.Tcp.Packets;

/// <summary>
/// The packet that is sent to check latency and confirm connection.
/// </summary>
public struct PingPacket : IByteSetSerializable {

    /// <summary>
    /// THe time at which the ping packet was sent from the sever.
    /// </summary>
    public DateTime Sent { get; set; }

    /// <summary>
    /// The size of the packet.
    /// </summary>
    public static int GetSize() => sizeof(long);

    public void Deserialize(ByteSetReader reader) {
        Sent = reader.GetDateTime();
    }

    public void Serialize(ByteSetWriter writer) {
        writer.Put(Sent);
    }
}
