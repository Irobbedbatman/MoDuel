using MoDuel.Json;
using MoDuel.Serialization;
using System.Collections.Frozen;
using System.Text.Json.Nodes;

namespace MoDuel.Data;

/// <summary>
/// Represents a loaded file that was in a json format.
/// </summary>
public abstract class LoadedAsset(Package package, string id, JsonObject? data = null) : IReloadable {

    /// <summary>
    /// The <see cref="Package"/> this <see cref="LoadedAsset"/> was loaded from.
    /// </summary>
    public readonly Package Package = package;

    /// <summary>
    /// The identifier this asset was loaded with.
    /// </summary>
    public readonly string Id = id;

    /// <summary>
    /// A name provided in <see cref="Data"/>.
    /// </summary>
    public readonly string Name = data?["Name"]?.ToString() ?? id;

    /// <summary>
    /// The json data that this was created from.
    /// </summary>
    public readonly JsonObject Data = data.ConvertNullToEmptyToken();

    /// <summary>
    /// The tags associated with this asset.
    /// </summary>
#nullable disable
    public readonly FrozenSet<string> Tags = data["tags"]?.AsArray().Select(t => t.ToRawValue<string>()).Where(t => t != null).ToFrozenSet();
#nullable enable

    /// <summary>
    /// Accessor for the any other value contained in the <see cref="Data"/>.
    /// </summary>
    public JsonNode this[string key] => Data.Get(key);

    /// <inheritdoc/>
    public string GetItemPath() => PackageCatalogue.GetFullItemPath(Package, Id);

    /// <summary>
    /// Checks to see if <see cref="Parameters"/>["Tags"] exists and contains <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <param name="caseSensitive">If the tag checks should retains case sensitive checking.</param>
    /// <returns></returns>
    public bool HasTag(string tag, bool caseSensitive = true) => Tags.Any(t => t.Equals(tag, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase));

}
