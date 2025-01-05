using System.Reflection;

namespace MoDuel.Client;

/// <summary>
/// Information to send to a client.
/// </summary>
public record ClientRequest {

    public readonly string RequestId;

    public readonly bool SendReadyConfirmation;

    public readonly object?[] Arguments;

    public ClientRequest(string requestId, bool sendReadyConfirmation, params object?[] arguments) {

        // TODO: get package of calling assembly to use as default package.

        RequestId = requestId;
        SendReadyConfirmation = sendReadyConfirmation;
        Arguments = arguments;
    }

    public ClientRequest(string requestId, params object?[] arguments) : this(requestId, false, arguments) { }

    /// <summary>
    /// Requires the players to send a ready confirmation when they receive this request.
    /// </summary>
    /// <returns>The new <see cref="ClientRequest"/></returns>
    public ClientRequest WithReadyConfirmation() {
        return new ClientRequest(RequestId, true, Arguments);
    }

}
