using static MoDuel.Shared.Logger;

namespace MoDuel.Tcp;

/// <summary>
/// Log types for this assembly.
/// </summary>
public static class LogTypes {

    /// <summary>
    /// The offset for the log type ids.
    /// </summary>
    private const int Offset = 1000;

    /// <summary>
    /// Logged when a connection status is changed.
    /// </summary>
    public static readonly LogType ConnectionStatus = new("Connection Status", 0 + Offset, false);
    /// <summary>
    /// Logged when data is received.
    /// </summary>
    public static readonly LogType DataReceived = new("Data Received", 1 + Offset);
    /// <summary>
    /// Logged when data is sent.
    /// </summary>
    public static readonly LogType DataSent = new("Data Sent", 2 + Offset, false);
    /// <summary>
    /// Logged when handling message size.
    /// </summary>
    public static readonly LogType MessageSize = new("Message Size", 3 + Offset, false);
    /// <summary>
    /// Logged when a ping is sent or received.
    /// </summary>
    public static readonly LogType Ping = new("Ping.", 4 + Offset, false);
    /// <summary>
    /// Logged when a ping is sent or received.
    /// </summary>
    public static readonly LogType Authentication = new("Authentication.", 5 + Offset);
}
