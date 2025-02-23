using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Networking.Messages;

/// <summary>
/// A request message sent in either direction.
/// </summary>
public abstract class Request : INetSerializable {

    /// <summary>
    /// The id of the request to execute.
    /// </summary>
    public readonly string RequestId;

    /// <summary>
    /// The arguments that will be passed.
    /// </summary>
    public readonly object?[] Arguments;

    public Request(string requestId, params object?[] args) {
        RequestId = requestId;
        Arguments = args;
    }

    /// <inheritdoc/>
    public void Deserialize(NetDataReader reader) {
        throw new NotImplementedException();
    }

    ///<inheritdoc/>
    public void Serialize(NetDataWriter writer) {
        throw new NotImplementedException();
    }
}
