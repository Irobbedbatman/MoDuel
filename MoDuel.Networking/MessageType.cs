namespace MoDuel.Networking;

/// <summary>
/// The message type ids shared from the client and the server.
/// </summary>
public enum MessagesType { 

    /// <summary>
    /// A request message sent either to the client or the server.
    /// </summary>
    Request,
    /// <summary>
    /// A response from the client to a <see cref="Request"/> made by the server.
    /// </summary>
    Response,
    /// <summary>
    /// A message sent to all connected clients that the duel is ready,
    /// </summary>
    GameReady,
    /// <summary>
    /// A message sent by the client to the server that contains their information.
    /// </summary>
    PlayerInfo,
    /// <summary>
    /// A response to a client based on their sent <see cref="PlayerInfo"/>.
    /// </summary>
    PlayerInfoResult,

} 
