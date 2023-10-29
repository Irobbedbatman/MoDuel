namespace MoDuel.Client;

/// <summary>
/// Information to send to a client.
/// </summary>
/// <param name="RequestId">The request id that the client can understand.</param>
/// <param name="SendReadyConfirmation">A flag that when true requires the client to return a ready confirmation message.</param>
/// <param name="Arguments">The data payload to send to the client for the request.</param>
public record ClientRequest(string RequestId, bool SendReadyConfirmation, params object?[] Arguments) {

    public ClientRequest(string RequestId, params object?[] Arguments) : this(RequestId, false, Arguments) { }

    /// <summary>
    /// Requires the players to send a ready confirmation when they recieve this request.
    /// </summary>
    /// <returns>The new <see cref="ClientRequest"/></returns>
    public ClientRequest WithReadyConfirmation() {
        return new ClientRequest(RequestId, true, Arguments);
    }

}
