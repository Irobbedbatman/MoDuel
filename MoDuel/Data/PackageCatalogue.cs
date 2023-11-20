using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Json;
using MoDuel.Resources;
using MoDuel.Serialization;
using MoDuel.Shared.Data;
using System.Text.Json.Nodes;

namespace MoDuel.Data;

/// <summary>
/// A dictionary of <see cref="Package"/>s and some Properties that define how it is browsed.
/// <para>There is no guarantee that the items in a package are available.</para>
/// <para>If a package is updated; it is better to create a new <see cref="PackageCatalogue"/> so that entitles using the old one don't lose access to information.</para>
/// </summary>
public class PackageCatalogue : BasePackageCatalogue<Package, PackageCatalogue>, IReloadable {

    /// <summary>
    /// Creates a <see cref="PackageCatalogue"/> by creating all the packages found at the <paramref name="locations"/>.
    /// </summary>
    public PackageCatalogue(ICollection<string> locations) {

        foreach (var location in locations) {
            if (location != null) {
                // Create the package at each location.
                Package? package = Package.LoadPackage(location, this);
                if (package != null) {
                    Packages.Add(package.Name, package);
                }
                else {
                    Console.WriteLine($"No package could be found at location: {location}");
                }
            }
        }

        // Set the default package to be the one named so or the one that has no name.
        DefaultPackage = this["Default"] ?? this["default"] ?? this[""] ?? null;
    }

    #region Package Content Loaders

    /// <summary>
    /// Uses the loader for the implemented generic type. Can also load <see cref="Package"/>s.
    /// <para>This is far slower that directly using the loader.</para>
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public T? Load<T>(string itemPath, Package? sourcePackage = null) {
        // Packages also can be retrieved but need to be checked first.
        if (typeof(T).IsAssignableFrom(typeof(Package))) {
            return (T?)(object?)GetPackage(itemPath);
        }
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        if (package == null) {
            return default;
        }
        return package.Load<T>(itemName);
    }

    /// <summary>
    /// Loads a <see cref="Card"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Card? LoadCard(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadCard(itemName);
    }

    /// <summary>
    /// Loads a <see cref="Hero"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Hero? LoadHero(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadHero(itemName);
    }

    /// <summary>
    /// Loads a <see cref="ActionFunction"/> from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public ActionFunction LoadAction(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadAction(itemName) ?? new ActionFunction();
    }

    /// <summary>
    /// Loads a json file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public JsonNode LoadJson(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadJson(itemName) ?? DeadToken.Instance;
    }

    /// <summary>
    /// Loads a <see cref="ResourceType"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public ResourceType? LoadResourceType(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadResourceType(itemName);
    }

    /// <summary>
    /// Try to load a <see cref="Card"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="Card"/> was able to be loaded.</returns>
    public bool TryGetCard(string fullItemPath, out Card? card) {
        card = LoadCard(fullItemPath);
        return card != null;
    }

    /// <summary>
    /// Try to load a <see cref="Hero"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="Hero"/> was able to be loaded.</returns>
    public bool TryGetHero(string fullItemPath, out Hero? hero) {
        hero = LoadHero(fullItemPath);
        return hero != null;
    }

    /// <summary>
    /// Try to load a <see cref="ActionFunction"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="ActionFunction"/> was able to be loaded.</returns>
    public bool TryGetAction(string fullItemPath, out ActionFunction? action) {
        action = LoadAction(fullItemPath);
        return action.IsAssigned;
    }

    /// <summary>
    /// Try to load a json object stored in a file.
    /// </summary>
    /// <returns><c>true</c> if the json file was able to be loaded.</returns>
    public bool TryGetJson(string fullItemPath, out JsonNode data) {
        data = LoadJson(fullItemPath);
        return !data.IsDead();
    }

    /// <summary>
    /// Try to load a <see cref="ResourceType"/> stored in a file.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="ResourceType"/> was loaded.</returns>
    public bool TryLoadResourceType(string fullItemPath, out ResourceType? resourceType) {
        resourceType = LoadResourceType(fullItemPath);
        return resourceType != null;
    }

    #endregion

}
