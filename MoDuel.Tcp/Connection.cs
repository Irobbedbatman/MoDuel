using MoDuel.Serialization;
using MoDuel.Shared;
using MoDuel.Tcp.Packets;
using System.Net.Sockets;

namespace MoDuel.Tcp;

/// <summary>
/// Base logic for a connection.
/// </summary>
public class Connection {

    /// <summary>
    /// The socket the connection is on.
    /// </summary>
    private readonly Socket Socket;

    /// <summary>
    /// The token to cancel the the connection.
    /// </summary>
    private readonly CancellationTokenSource CancellationTokenSource = new(TimeoutPeriod);

    /// <summary>
    /// The period for which messages should be received.
    /// </summary>
    public static int TimeoutPeriod { get; set; } = 1000 * 60 * 3;

    /// <summary>
    /// The size of the buffer used to send to the client.
    /// </summary>
    public static int ClientBufferSize { get; set; } = 1024;

    /// <summary>
    /// The timer that will cause a disconnect when reached.
    /// </summary>
    public readonly Timer DisconnectTimer;

    /// <summary>
    /// The current task that is invoked when a message is received.
    /// </summary>
    private Task<int>? MessageReceivedTask;

    /// <summary>
    /// The manager to help parse bytes into full messages.
    /// </summary>
    private MessageHeaderHandler MessageManager = new();

    /// <summary>
    /// Invoked when a message is received.
    /// </summary>
    public Action<ByteSetReader>? OnMessageReceived;
    /// <summary>
    /// Invoked when the connection is disconnected.
    /// </summary>
    public Action? OnDisconnected;
    /// <summary>
    /// Invoked when a ping message is received.
    /// </summary>
    public Action<double>? OnPing;

    protected bool disposedValue = false;

    public Connection(Socket socket) {
        Socket = socket;
        DisconnectTimer = new Timer(TimerElapsed);
        DisconnectTimer.Change(TimeoutPeriod, Timeout.Infinite);
        SetupMessageListener();
        SendPing();
    }

    /// <summary>
    /// Send the authentication message.
    /// </summary>
    public void SendAuthentication(string password, byte[] userData) {
        ByteSetWriter wrt = new();
        wrt.Put(new AuthenticationPacket() { Password = password, UserData = userData });
        Send(wrt.Data);
        Logger.Log(LogTypes.DataSent, $"Authentication sent at {DateTime.Now}");
    }

    /// <summary>
    /// Sends a ping to the connected client.
    /// </summary>
    private void SendPing() {
        ByteSetWriter wrt = new();
        wrt.Put(new PingPacket() { Sent = DateTime.UtcNow });
        var data = MessageHeaderHandler.AddSystemHeader(SystemMessageHeader.Ping, wrt.Data);
        SendRaw(data);
        Logger.Log(LogTypes.Ping, $"Ping Sent at {DateTime.Now}");
    }

    /// <summary>
    /// Handle a received message.
    /// </summary>
    protected virtual bool MessageReceived(ByteSetReader reader) {
        OnMessageReceived?.Invoke(reader);
        return true;
    }

    /// <summary>
    /// Handle received data.
    /// </summary>
    /// <param name="getLength">The length of bytes received.</param>
    /// <param name="buffer">The bytes received.</param>
    private void DataReceived(Task<int> getLength, ArraySegment<byte> buffer) {

        // Ensure the connection is still valid.
        if (getLength.IsCanceled || CancellationTokenSource.IsCancellationRequested) return;

        //Reset the timer when we receive a message from this client.
        DisconnectTimer.Change(TimeoutPeriod, Timeout.Infinite);

        int length;
        try {
            length = getLength.Result;
        }
        catch {
            length = -1;
        }
        if (length < 0) {
            NotifyDisconnect();
            Stop();
            return;
        }

        Logger.Log(LogTypes.MessageSize, $"Received Data Length: " + length);

        if (MessageManager.HandleIncomingData(buffer.AsSpan(0, length), out var messages)) {
            foreach (var message in messages) {

                var (messageType, messageContent) = message;

                Logger.Log(LogTypes.MessageSize, $"Received new message. Length: " + messageContent.Length);
                // Resolve the message, if there was an issue sto handling messages.
                switch (messageType) {
                    case SystemMessageHeader.Ping:
                        PingReceived(messageContent);
                        break;
                    default:
                        ByteSetReader reader = new ByteSetReader(messageContent);
                        bool messageWasHandled = MessageReceived(reader);
                        if (!messageWasHandled)
                            return;
                        break;
                }
            }
        };

        SetupMessageListener();
    }

    /// <summary>
    /// Handles a received ping message.
    /// </summary>
    private void PingReceived(byte[] messageContent) {
        ByteSetReader reader = new ByteSetReader(messageContent);
        var packet = reader.GetByteSetSerializable(() => new PingPacket());
        double timeTaken = (DateTime.UtcNow - packet.Sent).TotalMilliseconds;
        Logger.Log(LogTypes.Ping, $"Received ping packet: Round time: {timeTaken}ms");
        OnPing?.Invoke(timeTaken);
        SendPing();
    }

    /// <summary>
    /// Sets the message listener for the next message.
    /// </summary>
    private void SetupMessageListener() {
        byte[] buffer = new byte[ClientBufferSize];

        try {
            MessageReceivedTask = Socket.ReceiveAsync(buffer, cancellationToken: CancellationTokenSource.Token).AsTask();
            MessageReceivedTask.ContinueWith(t => {
                DataReceived(t, buffer);
            });
        }
        catch (SocketException) {
            Logger.Log(Logger.General, "Socket closed for client cannot accept further messages.");
        }
        catch {
            Logger.Log(Logger.General, "Objects disposed.");
        }
    }

    /// <summary>
    /// Called when the <see cref="DisconnectTimer"/> elapses.
    /// </summary>
    private void TimerElapsed(object? state) {
        CancellationTokenSource.Cancel();
        NotifyDisconnect();
        Stop();
    }

    /// <summary>
    /// Notify listener of disconnect on other end.
    /// </summary>
    private void NotifyDisconnect() {
        OnDisconnected?.Invoke();
        OnDisconnected = null;
    }

    /// <summary>
    /// Stop the connection.
    /// </summary>
    public virtual void Stop() {
        CancellationTokenSource.Cancel();
        OnMessageReceived = null;
        OnDisconnected = null;
        OnPing = null;
        Logger.Log(LogTypes.ConnectionStatus, "Connection terminated.");
    }

    /// <summary>
    /// Send a message without any additional headers.
    /// </summary>
    private void SendRaw(byte[] data) {
        try {
            Socket.SendAsync(data);
        }
        catch { }
    }

    /// <summary>
    /// Send a message to the connected socket.
    /// </summary>
    public void Send(byte[] data) {
        data = MessageHeaderHandler.AddHeader(data);
        SendRaw(data);
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                Stop();
            }
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
