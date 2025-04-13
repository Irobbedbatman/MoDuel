using MoDuel.Serialization;
using System.Net.Sockets;

namespace MoDuel.Tcp;

/// <summary>
/// A wrapper around <see cref="TcpClient"/> to handle higher lever operations.
/// </summary>
public class ManagedTcpClient : IDisposable {

    /// <summary>
    /// The internal client.
    /// </summary>
    private readonly TcpClient Client;

    /// <summary>
    /// The connection to the server.
    /// </summary>
    private Connection? Connection;

    private bool disposedValue;

    /// <summary>
    /// Action invoked when a client is disconnected.
    /// </summary>
    public Action? OnDisconnected;
    /// <summary>
    /// Action invoked whe a message is received.
    /// </summary>
    public Action<ByteSetReader>? OnMessageReceived;
    /// <summary>
    /// Action invoked when a ping is received, the value is the time difference in ms.
    /// </summary>
    public Action<double>? OnPing;

    public ManagedTcpClient() {
        Client = new TcpClient();
        Client.Client.NoDelay = false;
    }

    /// <summary>
    /// Connect to a tcp listener.
    /// </summary>
    /// <param name="address">The IP address of the listener.</param>
    /// <param name="port">The port of the listener.</param>
    /// <param name="password">A pass word that the listener is expecting for authentication.</param>
    /// <param name="userData">Data used in authentication.</param>
    public void Connect(string address, int port, string password = "", byte[]? userData = null) {
        try {
            Client.Connect(address, port);
            Connection = new Connection(Client.Client);
            Connection.SendAuthentication(password, userData ?? []);
            Connection.OnMessageReceived += (data) => {
                OnMessageReceived?.Invoke(data);
            };
            Connection.OnPing += (ms) => {
                OnPing?.Invoke(ms);
            };
            Connection.OnDisconnected += () => {
                OnDisconnected?.Invoke();
            };

        }
        catch { }
    }

    /// <summary>
    /// Send data to across the connection.
    /// </summary>
    /// <param name="data"></param>
    public void Send(byte[] data) {
        Connection?.Send(data);
    }

    /// <summary>
    /// Stop the connection.
    /// </summary>
    public void Stop() {
        OnDisconnected = null;
        OnMessageReceived = null;
        OnPing = null;
        Connection?.Dispose();
        Connection = null;
        Client.Close();
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            Stop();
            Client.Dispose();
            disposedValue = true;
        }
    }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
