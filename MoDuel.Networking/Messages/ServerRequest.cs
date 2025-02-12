namespace MoDuel.Networking.Messages;

/// <summary>
/// A request sent to the server.
/// </summary>
public class ServerRequest : Request {

    /// <summary>
    /// The package that created the request, informing the server which package should handle it.
    /// </summary>
    public readonly string? RequestingPackageId;

    public ServerRequest(string requestId, params object?[] args) : base(requestId, args) {

    }
}
