using MoDuel.Serialization;
using MoDuel.Shared;
using MoDuel.Tcp.Packets;
using System.Net.Sockets;

namespace MoDuel.Tcp;

/// <summary>
/// A connection on the host to a client.
/// </summary>
public class HostedConnection : Connection {

    /// <summary>
    /// The local host the client is connected to.
    /// </summary>
    public readonly ManagedTcpHost Host;

    /// <summary>
    /// The method used to authenticate a newly established connection.
    /// </summary>
    public Func<AuthenticationPacket, bool>? Authenticator;

    /// <summary>
    /// Indicates the connection has been authenticated and can handle other messages.
    /// </summary>
    public bool Authenticated { get; private set; } = false;

    public HostedConnection(ManagedTcpHost host, TcpClient client) : base(client.Client) {
        Host = host;
    }

    /// <inheritdoc/>
    protected override bool MessageReceived(ByteSetReader reader) {
        // First message a host should receive from a client is n authentication packet.
        if (!Authenticated)
            return Authenticate(reader);
        return base.MessageReceived(reader);
    }

    /// <summary>
    /// Authenticate the current connection by using the <see cref="Authenticator"/>.
    /// </summary>
    private bool Authenticate(ByteSetReader reader) {
        AuthenticationPacket packet;
        try {
            packet = reader.GetByteSetSerializable(() => new AuthenticationPacket());
        }
        catch {
            Logger.Log(LogTypes.Authentication, "Received incorrect packet, expected an authentication packet.");
            return false;
        }
        bool successful = Authenticator?.Invoke(packet) ?? true;
        if (successful) {
            Logger.Log(LogTypes.Authentication, "Received authentication packet. Success.");
            Authenticated = true;
            return true;
        }
        Logger.Log(LogTypes.Authentication, "Received authentication packet. Failed.");
        Stop();
        return false;
    }

    /// <summary>
    /// Stop the connection and clear any signals.
    /// </summary>
    public override void Stop() {
        Authenticator = null;
        try {
            Host.ActiveConnections.TryRemove(this, out _);
        }
        catch { }
        base.Stop();
    }

}
