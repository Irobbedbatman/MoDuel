using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Networking.Messages;

/// <summary>
/// A message that will be sent to all listening clients.
/// </summary>
/// <typeparam name="T">The internal message to be broadcast.</typeparam>
internal class BroadcastMessage<T> where T : INetSerializable {

    /// <summary>
    /// The internal message request.
    /// </summary>
    public T InnerMessage;

    /// <summary>
    /// The set of players the message wont be sent to.
    /// </summary>
    public string[] ExemptPlayers;

    public BroadcastMessage(T innerMessage, string[]? exemptPlayers = null) {
        InnerMessage = innerMessage;
        ExemptPlayers = exemptPlayers ?? [];
    }
}
