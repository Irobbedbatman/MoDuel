using MoDuel.Json;
using MoDuel.Serialization;
using Newtonsoft.Json.Linq;

namespace MoDuel.Data;


/// <summary>
/// Represenets a loaded file that was in a json format.
/// </summary>
public abstract class LoadedAsset : IReloadable {

    /// <summary>
    /// The <see cref="Package"/> this <see cref="LoadedAsset"/> was loaded from.
    /// </summary>
    public readonly Package Package;

    /// <summary>
    /// The identifier this asset was loaded with.
    /// </summary>
    public readonly string Id;

    /// <summary>
    /// A name provided in <see cref="Data"/>.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The json data that this was created from.
    /// </summary>
    public readonly JObject Data;

    public LoadedAsset(Package package, string id, JObject? data = null) {
        Package = package;
        Id = id;
        Data = data.ConvertNullToEmptyToken();
        Name = data?["Name"]?.ToString() ?? id;
    }

    /// <summary>
    /// Accessor for the any other value contained in the <see cref="Data"/>.
    /// </summary>
    public JToken this[string key] => Data.TryGet(key);

    /// <inheritdoc/>
    public string GetItemPath() => PackageCatalogue.GetFullItemPath(Package, Id);

    /// <summary>
    /// Checks to see if <see cref="Parameters"/>["Tags"] exists and contains <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    /// <param name="caseSensitive">If the tag checks should retian case sensitive checking.</param>
    /// <returns></returns>
    public bool HasTag(string tag, bool caseSensitive = true) => this["tags"].ContainsRawValue(tag, caseSensitive);


}
