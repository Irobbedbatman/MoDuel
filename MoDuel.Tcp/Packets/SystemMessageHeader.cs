namespace MoDuel.Tcp.Packets;

/// <summary>
/// Optional headers attacked to messages in place of size, Used to label them as system messages.
/// </summary>
public enum SystemMessageHeader {
    Standard = 0,
    Error = -1,
    Ping = -2
}
