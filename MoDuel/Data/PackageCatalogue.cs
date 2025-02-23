using MoDuel.Abilities;
using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Resources;
using MoDuel.Serialization;
using MoDuel.Shared;
using MoDuel.Shared.Data;
using MoDuel.Shared.Json;
using System.Diagnostics.CodeAnalysis;
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
                    Logger.Log(Logger.DataLoadingError, $"No package could be found at location: {location}");
                }
            }
        }

        // Set the default package to be the one named so or the one that has no name.
        DefaultPackage = this["Default"] ?? this["default"] ?? this[""] ?? null;
    }

    #region Package Content Loaders

    /// <summary>
    /// Loads a <see cref="Card"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Card? LoadCard(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, Package.CARD_CATEGORY, out string itemName, sourcePackage);
        return package?.LoadCard(itemName);
    }

    /// <summary>
    /// Loads a <see cref="Hero"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Hero? LoadHero(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, Package.HERO_CATEGORY, out string itemName, sourcePackage);
        return package?.LoadHero(itemName);
    }

    /// <summary>
    /// Loads a <see cref="ActionFunction"/> from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public ActionFunction LoadAction(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, Package.ACTION_CATEGORY, out string itemName, sourcePackage);
        return package?.LoadAction(itemName) ?? new ActionFunction();
    }

    /// <summary>
    /// Loads a <see cref="Ability"/> from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Ability LoadAbility(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, Package.ABILITY_CATEGORY, out string itemName, sourcePackage);
        return package?.LoadAbility(itemName) ?? Ability.Missing;
    }

    /// <summary>
    /// Loads a json file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public JsonNode LoadJson(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, Package.JSON_DATA_CATEGORY, out string itemName, sourcePackage);
        return package?.LoadJson(itemName) ?? DeadToken.Instance;
    }

    /// <summary>
    /// Loads a <see cref="ResourceType"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is separated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public ResourceType? LoadResourceType(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, Package.RESOURCE_TYPE_CATEGORY, out string itemName, sourcePackage);
        return package?.LoadResourceType(itemName);
    }

    /// <summary>
    /// Try to load a <see cref="Card"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="Card"/> was able to be loaded.</returns>
    public bool TryGetCard(string fullItemPath, [NotNullWhen(true)] out Card? card) {
        card = LoadCard(fullItemPath);
        return card != null;
    }

    /// <summary>
    /// Try to load a <see cref="Hero"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="Hero"/> was able to be loaded.</returns>
    public bool TryGetHero(string fullItemPath, [NotNullWhen(true)] out Hero? hero) {
        hero = LoadHero(fullItemPath);
        return hero != null;
    }

    /// <summary>
    /// Try to load a <see cref="ActionFunction"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="ActionFunction"/> was able to be loaded.</returns>
    public bool TryGetAction(string fullItemPath, [NotNullWhen(true)] out ActionFunction? action) {
        action = LoadAction(fullItemPath);
        return action.IsAssigned;
    }

    /// <summary>
    /// Try to load a <see cref="Ability"/>.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="Ability"/> was able to be loaded.</returns>
    public bool TryGetAbility(string fullItemPath, [NotNullWhen(true)] out Ability? ability) {
        ability = LoadAbility(fullItemPath);
        return ability != Ability.Missing;
    }

    /// <summary>
    /// Try to load a json object stored in a file.
    /// </summary>
    /// <returns><c>true</c> if the json file was able to be loaded.</returns>
    public bool TryGetJson(string fullItemPath, [NotNullWhen(true)] out JsonNode data) {
        data = LoadJson(fullItemPath);
        return !data.IsDead();
    }

    /// <summary>
    /// Try to load a <see cref="ResourceType"/> stored in a file.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="ResourceType"/> was loaded.</returns>
    public bool TryGetResourceType(string fullItemPath, [NotNullWhen(true)] out ResourceType? resourceType) {
        resourceType = LoadResourceType(fullItemPath);
        return resourceType != null;
    }

    #endregion

    /// <summary>
    /// Get all the resource types that have been loaded for all the loaded packages.
    /// </summary>
    public IEnumerable<ResourceType> GetAllLoadedResourceTypes() {
        return Packages.SelectMany(p => p.Value.GetLoadedResourceTypes());
    }


}
