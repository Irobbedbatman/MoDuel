using MoDuel.Shared.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MoDuel.Shared.Data;

/// <summary>
/// The base structure of a package.
/// </summary>
/// <typeparam name="P">The type that will be used as the package.</typeparam>
/// <typeparam name="C">The type that will be used as the catalogue, which will contain this package..</typeparam>
public abstract class BasePackage<P, C> where C : BasePackageCatalogue<P, C> where P : BasePackage<P, C> {

    /// <summary>
    /// The name of the package and the key used to represent it in short form.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The base folder where the package info has been listed.
    /// </summary>
    public readonly string Directory;

    /// <summary>
    /// The JSON data that was found in the package file. 
    /// </summary>
    public readonly JsonObject Data;

    /// <summary>
    /// Shorthand accessor for <see cref="Data"/>.
    /// </summary>
    public JsonNode this[string key] => Data.Get(key);

    /// <summary>
    /// The list of of packages that includes this package used to load cross package content.
    /// </summary>
    public readonly C Catalogue;

    protected BasePackage(string name, string directory, JsonObject data, C catalogue) {
        Name = name;
        Directory = directory;
        Data = data;
        Catalogue = catalogue;
    }

    /// <summary>
    /// Retrieves a full path to a file.
    /// </summary>
    /// <param name="category">The category the file is listed under.</param>
    /// <param name="index">The key that is used in the category to get the result.</param>
    /// <returns><c>null</c> if there was no valid <paramref name="category"/> or the <paramref name="index"/> wasn't found.</returns>
    public string? GetFullSystemPath(string category, string index) {
        // Get the path to the item from the package directory.
        var relativePath = GetRelativeSystemPath(category, index);
        if (relativePath == null) return null;
        return Path.Combine(Directory, relativePath);
    }

    /// <summary>
    /// Retrieves a relative path to a file.
    /// </summary>
    /// <param name="category">The category the file is listed under.</param>
    /// <param name="index">The key that is used in the category to get the result.</param>
    /// <returns><c>null</c> if there was no valid <paramref name="category"/> or the <paramref name="index"/> wasn't found.</returns>
    public string? GetRelativeSystemPath(string category, string index) {
        // Ensure the category exists.
        if (Data.TryGet(category, out var categoryToken)) {
            // Ensure the category is actually a category.
            if (categoryToken is JsonObject categoryValue) {
                // Check to see if the index is supplied within the category.
                if (categoryValue.TryGet(index, out var locationToken)) {
                    // Ensure the token is a string value.
                    if (locationToken.GetValueKind() == JsonValueKind.String)
                        return locationToken.ToString();
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Converts a relative system path to a full system path.
    /// </summary>
    public string GetFullSystemPath(string relativePath) => Path.Combine(Directory, relativePath);

    /// <summary>
    /// Retrieves the relative and full path to a file using <see cref="GetRelativeSystemPath(string, string)"/>.
    /// <para>Outputs both the relative and the full path.</para>
    /// <para>Ensures the file exists.</para>
    /// </summary>
    /// <param name="category">The category within the <see cref="Package"/> to load from.</param>
    /// <param name="id">The unique id within the category.</param>
    /// <param name="relativePath">The relative path that was found or null if no path was found.</param>
    /// <param name="fullPath">The full path that was found or null if no path was found.</param>
    /// <returns>True if the paths a re valid and the file exists; false otherwise.</returns>
    public bool GetSystemPaths(string category, string id, out string? relativePath, out string? fullPath) {
        relativePath = GetRelativeSystemPath(category, id);
        if (relativePath == null) {
            fullPath = null;
            return false;
        }
        fullPath = GetFullSystemPath(relativePath);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// Use <see cref="Name"/> for hashing. This means that packages require unique names.
    /// </summary>
    public override int GetHashCode() {
        return Name.GetHashCode();
    }

    /// <summary>
    /// Use <see cref="Name"/> for quality. As there should not be multiple <see cref="Package"/>s with the same name.
    /// </summary>
    public override bool Equals(object? obj) {
        if (obj is P package) {
            return Name.Equals(package.Name);
        }
        return base.Equals(obj);
    }

    /// <summary>
    /// Retrieves all the items that can be found in the provided <paramref name="category"/>.
    /// <returns>The values found or an empty <see cref="IEnumerable{T}"/> if the category didn't exist or has no values.</returns>
    public IEnumerable<string> GetAllItemNamesInCategory(string category) {

        // Get the data values of the category.
        JsonNode? values = Data[category];

        // Ensure it is valid.
        if (values == null)
            return [];

        return values.GetKeys();
    }

    /// <summary>
    /// Returns the value of the property provided; properties are always strings.
    /// </summary>
    public T? GetProperty<T>(string propertyName) {
        var value = Data[propertyName];
        if (value?.ToRawValue() is T tValue) {
            return tValue;
        }
        return default;
    }

    /// <summary>
    /// Uses the property provided as a path to return the full path from the package.
    /// </summary>
    public string? GetFilePropertyPath(string propertyName) {
        var subPath = GetProperty<string>(propertyName);
        if (subPath == null) return null;
        return Path.GetFullPath(Path.Combine(Directory, subPath));
    }

    /// <inheritdoc/>
    public string? GetItemPath() {
        return Name;
    }

}
