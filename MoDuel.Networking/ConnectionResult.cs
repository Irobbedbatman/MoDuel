using LiteNetLib.Utils;

namespace MoDuel.Networking;

/// <summary>
/// The result message on a connection attempt.
/// </summary>
public class ConnectionResult : INetSerializable {

    /// <summary>
    /// Whether the connection was successful.
    /// </summary>
    public bool Success => ResponseCode < 0;

    /// <summary>
    /// The response of the connection. Negative codes indicate success, positive codes represent errors.
    /// </summary>
    public int ResponseCode;

    public void Deserialize(NetDataReader reader) {
        ResponseCode = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer) {
        writer.Put(ResponseCode);
    }
}
