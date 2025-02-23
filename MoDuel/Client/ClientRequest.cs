using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Networking.Messages;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MoDuel.Client;

/// <summary>
/// Information to send to a client.
/// </summary>
public class ClientRequest : Request {

    /// <summary>
    /// Do the clients need to send a response to this request.
    /// </summary>
    public readonly bool SendReadyConfirmation;

    /// <summary>
    /// Which package sent the request.
    /// </summary>
    public readonly Package? RequestingPackage;

    public ClientRequest(string requestId, Package? requestingPackage, params object?[] arguments) : base(requestId, arguments) {
        RequestingPackage = requestingPackage;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ClientRequest(string requestId, params object?[] arguments) : base(requestId, arguments) {
        var callingAssembly = Assembly.GetCallingAssembly();
        var packagedCode = PackageAssemblyAttribute.GetPackage(callingAssembly);
        RequestingPackage = packagedCode?.Package;
    }

    /// <summary>
    /// Requires the players to send a ready confirmation when they receive this request.
    /// </summary>
    /// <returns>The new <see cref="ClientRequest"/></returns>
    public ClientRequest WithReadyConfirmation() {
        return new ClientRequest(RequestId, RequestingPackage, Arguments);
    }

}
