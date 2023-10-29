using MoDuel.Json;
using MoDuel.Serialization;
using Newtonsoft.Json.Linq;

namespace MoDuel.Data;


/// <summary>
/// A json block of data that has been opened that holds refrences to files and file keys.
/// <para>To get files paths from a certain category within the package use <see cref="GetFullSystemPath(string, string)"/>.</para>
/// <para>Thsis class is partial the loaded content can be found in PackageContent.cs</para>
/// </summary>
public partial class Package : IReloadable {

    /// <summary>
    /// The name of the package and the key used to represent it in short form.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The base folder wheere the package info has been listed.
    /// </summary>
    public readonly string Directory;

    /// <summary>
    /// The JSON data that was found in the package file. 
    /// </summary>
    public readonly JObject Data;

    /// <summary>
    /// Shorthand accessor for <see cref="Data"/>.
    /// </summary>
    public JToken this[string key] => Data.TryGet(key);

    /// <summary>
    /// The list of of packages that includes this package used to load cross package content.
    /// </summary>
    public readonly PackageCatalogue Catalogue;

    private Package(string name, string directory, JObject data, PackageCatalogue? catalogue = null) {
        Name = name;
        Directory = directory;
        Data = data;
        PackagedCode = PackagedCodeFinder.FindAndInit(this);
        Catalogue = catalogue ?? new PackageCatalogue(this);
    }

    /// <summary>
    /// Creates a new <see cref="Package"/> from the file found at <paramref name="path"/>.
    /// </summary>
    /// <param name="loadCode">Set to true to load the code package later to resolve any recursive loading.</param>
    /// <exception cref="FileNotFoundException"/>
    /// <returns>A new <see cref="Package"/> if the file was correct and the package was created; otherwise <c>null</c>.</returns>
    public static Package? LoadPackage(string path, PackageCatalogue? catalogue = null) {

        JObject data;

        // Ensure data is loaded.
        try {
            data = JObject.Parse(File.ReadAllText(path));
        }
        catch {
            Console.WriteLine($"Package at {path} could not be found or loaded.");
            return null;
        }

        // Get the name of the package.
        string name = data?["Name"]?.ToString() ?? "Error Package";

        var package = new Package(
            name,
            Path.GetDirectoryName(path) ?? "",
            data ?? new JObject(),
            catalogue
        );

        package.PackagedCode.Load();

        return package;
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
        if (Data.TryGetValue(category, out var categoryToken)) {
            // Ensure the category is actually a category.
            if (categoryToken is JObject categoryValue) {
                // Check to see if the index is supplied within the category.
                if (categoryValue.TryGetValue(index, out var locationToken)) {
                    // Ensure the token is a string value.
                    if (locationToken.Type == JTokenType.String)
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
    /// Retreives the relative and full path to a file using <see cref="GetRelativeSystemPath(string, string)"/>.
    /// <para>Outputs both the relative and the full path.</para>
    /// <para>Ensures the file exists.</para>
    /// </summary>
    /// <param name="category">The category within the <see cref="Package"/> to load from.</param>
    /// <param name="id">The unique id within the category.</param>
    /// <param name="relativePath">The relative path that was found or null if no path was found.</param>
    /// <param name="fullPath">The full path that was found or null if no path was found.</param>
    /// <returns>True if the paths a re valid and the file exists; false otherwise.</returns>
    private bool GetSystemPaths(string category, string id, out string? relativePath, out string? fullPath) {
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
        if (obj is Package package) {
            return Name.Equals(package.Name);
        }
        return base.Equals(obj);
    }

    /// <summary>
    /// Retrieves all the items that can be found in the provided <paramref name="category"/>.
    /// <returns>The values found or an empty <see cref="IEnumerable{T}"/> if the category didn't exist or has no values.</returns>
    public IEnumerable<string> GetAllItemNamesInCategory(string category) {

        // Get the data values of the category.
        JToken? values = Data[category];

        // Ensure it is valid.
        if (values == null || !values.HasValues)
            return Array.Empty<string>();


        return values.OfType<JProperty>().Select((value) => {
            return value.Name;
        });
    }

    /// <summary>
    /// Returns the value of the property provided; properties are always strings.
    /// </summary>
    public T? GetProperty<T>(string propertyName) {
        var value = Data[propertyName];
        if (value is JValue jValue) {
            if (jValue.ToRawValue() is T tValue) {
                return tValue;
            }
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
