using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Networking.Messages;

/// <summary>
/// A request message that will be sent to all listening clients.
/// </summary>
/// <typeparam name="T">The internal message to be broadcast.</typeparam>
internal class BroadcastRequest<T> : Request where T : Request{

    /// <summary>
    /// The internal message request.
    /// </summary>
    public T BaseRequest;

    /// <summary>
    /// The set of players the message wont be sent to.
    /// </summary>
    public string[] ExemptPlayers;

    public BroadcastRequest(T request, string[]? exemptPlayers = null) : base(request.RequestId, request.Arguments) {
        BaseRequest = request;
        ExemptPlayers = exemptPlayers ?? [];
    }
}
