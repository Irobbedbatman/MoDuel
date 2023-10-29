using MoDuel.Serialization;
using Newtonsoft.Json.Linq;

namespace MoDuel.Json;


/// <summary>
/// A proxy for a <see cref="JToken"/> that does not store that jtoken.
/// <para>This is important to use for serialization as only the <see cref="Root"/> needs to be heavily serialized.</para>
/// </summary>
[SerializeReference]
public class JsonProxy {

    /// <summary>
    /// Wether the <see cref="JsonProxy"/> should return to a <see cref="JToken"/> after deserialization.
    /// </summary>
    internal bool ForceToToken = false;

    /// <summary>
    /// The root node that <see cref="Path"/> will branch from.
    /// </summary>
    public readonly JToken Root;

    /// <summary>
    /// The path to the node from the <see cref="Root"/>.
    /// </summary>
    public readonly string Path;

    public JsonProxy(JToken subValue) {
        Root = subValue.Root;
        Path = subValue.Path;
    }

    /// <summary>
    /// Access to re-retreive the token once wrapped.
    /// </summary>
    public JToken Token => Root.SelectToken(Path).ConvertNullToDeadToken();

    /// <inheritdoc/>
    public override bool Equals(object? obj) {
        return obj is JsonProxy proxy &&
               Root == proxy.Root &&
               Path == proxy.Path;
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
        return HashCode.Combine(Root, Path);
    }
}
