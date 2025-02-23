using MoDuel.Data.Assembled;
using MoDuel.Serialization;
using MoDuel.Shared;
using MoDuel.Shared.Data;
using System.Text.Json.Nodes;

namespace MoDuel.Data;


/// <summary>
/// A json block of data that has been opened that holds references to files and file keys.
/// <para>To get files paths from a certain category within the package use <see cref="GetFullSystemPath(string, string)"/>.</para>
/// <para>This class is partial the loaded content can be found in PackageContent.cs</para>
/// </summary>
public partial class Package : BasePackage<Package, PackageCatalogue>, IReloadable {

    private Package(string name, string directory, JsonObject data, PackageCatalogue catalogue) : base(name, directory, data, catalogue) {
        PackagedCode = PackagedCodeFinder.FindAndInit(this);
    }

    /// <summary>
    /// Creates a new <see cref="Package"/> from the file found at <paramref name="path"/>.
    /// </summary>
    /// <param name="loadCode">Set to true to load the code package later to resolve any recursive loading.</param>
    /// <exception cref="FileNotFoundException"/>
    /// <returns>A new <see cref="Package"/> if the file was correct and the package was created; otherwise <c>null</c>.</returns>
    public static Package? LoadPackage(string path, PackageCatalogue catalogue) {

        JsonObject data;

        // Ensure data is loaded.
        try {
            data = (JsonObject?)JsonNode.Parse(File.ReadAllText(path)) ?? throw new NullReferenceException();
        }
        catch {
            Logger.Log(Logger.DataLoadingError, $"Package at {path} could not be found or loaded.");
            return null;
        }

        // Get the name of the package.
        string name = data?["Name"]?.ToString() ?? "Error Package";

        var package = new Package(
            name,
            Path.GetDirectoryName(path) ?? "",
            data ?? [],
            catalogue
        );

        package.PackagedCode.Load();

        return package;
    }

}
