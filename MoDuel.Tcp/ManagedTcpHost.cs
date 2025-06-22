using MoDuel.Serialization;
using MoDuel.Shared;
using MoDuel.Tcp.Packets;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MoDuel.Tcp;

/// <summary>
/// A wrapper around <see cref="TcpListener"/> to handle higher lever operations.
/// </summary>
public class ManagedTcpHost : IDisposable {

    /// <summary>
    /// The internal host.
    /// </summary>
    public readonly TcpListener Host;

    /// <summary>
    /// The list of currently active client connections.
    /// <para>The value column does not represent anything and should be 0.</para>
    /// </summary>
    internal readonly ConcurrentDictionary<HostedConnection, byte> ActiveConnections = [];

    /// <summary>
    /// THe local port used for the connections. -1 if the host is not active.
    /// </summary>
    public int Port => (Host.Server.LocalEndPoint as IPEndPoint)?.Port ?? -1;

    /// <summary>
    /// Task that is executed when a new connection is established.
    /// </summary>
    private Task<TcpClient>? connectionTask;
    private bool disposedValue;

    /// <summary>
    /// The cancellation token to cancel the connection and any close any communication channels.
    /// </summary>
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// Action invoked when a client is connected.
    /// </summary>
    public Action<HostedConnection>? OnClientConnected;

    /// <summary>
    /// Action invoked when a client is disconnected.
    /// </summary>
    public Action<HostedConnection>? OnClientDisconnected;

    /// <summary>
    /// Action invoked whe a message is received.
    /// </summary>
    public Action<HostedConnection, ByteSetReader>? OnMessageReceived;

    /// <summary>
    /// Action invoked when a ping is received, the value is the time difference in ms.
    /// </summary>
    public Action<HostedConnection, double>? OnPing;

    /// <summary>
    /// Function used to authenticate an incoming transactions before handling other messages.
    /// </summary>
    public Func<HostedConnection, AuthenticationPacket, bool>? Authenticator;


    public ManagedTcpHost(int port = 0) {
        // Start the host.
        Host = new TcpListener(IPAddress.Any, port);
        Host.Server.NoDelay = true;
    }

    /// <summary>
    /// Start the host.
    /// </summary>
    public void Start() {

        if (Port != -1) {
            Logger.Log(LogTypes.ConnectionStatus, "Host already started on port: " + Port);
            return;
        }

        Host.Start();
        Logger.Log(LogTypes.ConnectionStatus, "Host started on port: " + Port);

        // Create the task for when a client has connected.
        connectionTask = Host.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();
        connectionTask.ContinueWith(OnNewClientConnected);
    }

    /// <summary>
    /// Action called when a new client is connected.
    /// </summary>
    /// <param name="task">The task to get the new client.</param>
    private void OnNewClientConnected(Task<TcpClient> task) {
        if (task.IsCanceled || cancellationTokenSource.IsCancellationRequested) return;

        try {
            connectionTask = Host.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();
            connectionTask.ContinueWith(OnNewClientConnected);
        }
        catch (Exception ex) {
            Logger.Log(Logger.General, "Host stopped. No longer accepting new connections.");
            return;
        }
        var client = task.Result;

        Logger.Log(Logger.General, "Client connected. Client: " + client.Client.RemoteEndPoint);

        HostedConnection connection = new(this, client);
        connection.OnMessageReceived += (data) => {
            OnMessageReceived?.Invoke(connection, data);
        };
        connection.OnPing += (ms) => {
            OnPing?.Invoke(connection, ms);
        };
        connection.OnDisconnected += () => {
            OnClientDisconnected?.Invoke(connection);
        };
        connection.Authenticator += (data) => {
            return Authenticator?.Invoke(connection, data) ?? true;
        };

        ActiveConnections.TryAdd(connection, 0);
        OnClientConnected?.Invoke(connection);

        Logger.Log(Logger.General, $"Connected Clients: {ActiveConnections.Count}");
    }

    /// <summary>
    /// Stop the host.
    /// </summary>
    public void Stop() {
        cancellationTokenSource.Cancel();
        foreach (var connection in ActiveConnections.Keys) {
            connection.Stop();
        }
        Host.Stop();
        Console.WriteLine("Host stopped.");
    }

    /// <summary>
    /// Send a message to all active clients.
    /// </summary>
    public void SendToAll(byte[] data) {

        var clients = ActiveConnections.Keys;

        Logger.Log(Logger.General, $"Sending a message to {clients.Count} connected clients.");
        foreach (var client in clients) {
            try {
                client.Send(data);
            }
            catch {
                Logger.Log(Logger.General, "Failed to send to a client. Disconnecting them.");
                client.Stop();
            }
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // Dispose managed state (managed objects)
            }

            Stop();
            OnMessageReceived = null;
            OnClientConnected = null;
            OnClientDisconnected = null;
            Authenticator = null;
            connectionTask?.ConfigureAwait(false);
            connectionTask = null;
            cancellationTokenSource.Dispose();
            disposedValue = true;
        }
    }


    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
