using System.Text.Json.Nodes;

namespace MoDuel.Shared.Json;

/// <summary>
/// A proxy for a <see cref="JToken"/> that does not store that json token; instead storing the path.
/// </summary>
public class JsonProxy(JsonNode subValue) {

    /// <summary>
    /// Whether the <see cref="JsonProxy"/> should return to a <see cref="JsonNode"/> after deserialization.
    /// </summary>
    internal bool ForceToToken = false;

    /// <summary>
    /// The root node that <see cref="Path"/> will branch from.
    /// </summary>
    public readonly JsonNode Root = subValue.Root;

    /// <summary>
    /// The path to the node from the <see cref="Root"/>.
    /// </summary>
    public readonly string Path = subValue.GetPath();

    /// <summary>
    /// Access to re-retrieve the token once wrapped.
    /// </summary>
    public JsonNode Token => Root.GetUsingPath(Path);

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
