using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Json;
using MoDuel.Resources;
using MoDuel.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace MoDuel.Data;


/// <summary>
/// A dictionary of <see cref="Package"/>s and some Properties that define how it is browsed.
/// <para>There is no guarentee that the items in a package are available.</para>
/// <para>If a package is updated; it is better to create a new <see cref="PackageCatalogue"/> so that entites using the old one don't lose access to information.</para>
/// </summary>
public class PackageCatalogue : IReloadable, IEnumerable<Package> {

    /// <summary>
    /// Access to each <see cref="Package"/> via it's <see cref="Package.Name"/>
    /// </summary>
    private readonly Dictionary<string, Package> Catalogue = new();

    /// <summary>
    /// The list of all the packages stored in the catalogue.
    /// </summary>
    public IReadOnlyList<Package> AllPackages => Catalogue.Values.ToList();

    /// <summary>
    /// The separator that seperates packages from their items. I.e: package|item
    /// </summary>
    public const char PackageItemSeparator = '|';
    /// <summary>
    /// An alterntive way to access the <see cref="DefaultPackage"/> instead of using it's name.
    /// </summary>
    public const string DeafultPackageIndicator = ">";
    /// <summary>
    /// An alternative way to access packages item from the same package instead of requiring the package name. Note: this is the default behaviour when no package name is provided.
    /// </summary>
    public const string SamePackageIndicator = "?>";
    /// <summary>
    /// A content package that is more accesible then the others. 
    /// </summary>
    public Package? DefaultPackage = null;

    /// <summary>
    /// Constructor of a self container package.
    /// </summary>
    internal PackageCatalogue(Package package) {
        Catalogue.Add(package.Name, package);
        DefaultPackage = package;
    }

    /// <summary>
    /// Creates a <see cref="PackageCatalogue"/> by creating all the packages found at the <paramref name="locations"/>.
    /// </summary>
    public PackageCatalogue(ICollection<string> locations) {

        foreach (var location in locations) {
            if (location != null) {
                // Create the package at each location.
                Package? package = Package.LoadPackage(location);
                if (package != null) {
                    Catalogue.Add(package.Name, package);
                }
                else {
                    Console.WriteLine($"No package could be found at location: {location}");
                }
            }
        }

        // Set the default package to be the one named so or the one that has no name.
        DefaultPackage = this["Default"] ?? this["default"] ?? this[""] ?? null;
    }

    /// <summary>
    /// Accessor for the a <see cref="Package"/> in the catalouge.
    /// </summary>
    public Package? this[string packageName] => GetPackage(packageName);

    /// <summary>
    /// Recreates the full item path of an item.
    /// <para>The item path is: <paramref name="package"/>.Name + <see cref="PackageItemSeparator"/> + <paramref name="itemName"/>.</para>
    /// </summary>
    /// <param name="package">The package the item was found in.</param>
    /// <param name="itemName">The name of the item inside the package.</param>
    /// <returns>The full path to the item.</returns>
    public static string GetFullItemPath(Package package, string itemName) {
        return string.Concat(package.Name, PackageItemSeparator, itemName);
    }

    /// <summary>
    /// Retrieves the package with the provided name if it exists.
    /// </summary>
    public Package? GetPackage(string packageName) => Catalogue.TryGetValue(packageName ?? "", out var value) ? value : null;

    /// <summary>
    /// Retrieves a package from an <paramref name="itemPath"/> and also keeps the <paramref name="itemName"/> as a remainder of that path.
    /// <para>By not providing a full <paramref name="itemPath"/> and instead only a <paramref name="itemName"/> the returned package will be the <paramref name="currentPackage"/> if it exists or <see cref="DefaultPackage"/></para>
    /// </summary>
    /// <param name="itemPath">
    /// A coded path that should be the <see cref="Package.Name"/> followed by the <see cref="PackageItemSeparator"/> then the <paramref name="itemName"/>.
    /// <para>Only an <paramref name="itemName"/> could also be provided; this will use the <paramref name="currentPackage"/> if it exists or <see cref="DefaultPackage"/>.</para>
    /// </param>
    /// <param name="itemName">The remaining portion of <paramref name="itemPath"/> that is the item requested inside the package.</param>
    /// <param name="currentPackage">The package that is requesting the package; used if the requested item is in the same package.</param>
    /// <returns>The <see cref="Package"/> that contains the item requested.</returns>
    public Package? GetPackageFromItemPath(string itemPath, out string itemName, Package? currentPackage = null) {

        // Seperate the packagename and the item name.
        var pathSplit = itemPath.Split(PackageItemSeparator);

        if (pathSplit.Length == 1) {
            // Item name is all that was provided.
            itemName = itemPath;
            // If this was not requested from a package assume the item is in the default package.
            if (currentPackage == null) {
                return DefaultPackage;
            }
            // If this was requested from a package assume the item is from that same paackage.
            return currentPackage;
        }

        // Should only be a package id or a package id and item.
        if (pathSplit.Length > 2) {
            itemName = "";
            return null;
        }

        // Get the packageName
        string packageName = pathSplit[0];
        // Get the itemName
        itemName = pathSplit[1];

        // If the packageName was instead an indicator instead use indicated package.
        if (packageName == DeafultPackageIndicator) {
            return DefaultPackage;
        }
        if (packageName == SamePackageIndicator) {
            // If it was a same package request require there be a same package to use.
            if (currentPackage != null)
                return currentPackage;
            return null;
        }

        // Get the ContentPackage based on it's name.
        return GetPackage(packageName);

    }

    #region Cross Package Content Loaders


    /// <summary>
    /// Uses the loader for the implmented generic type. Can also load <see cref="Package"/>s.
    /// <para>This is far slower that directly using the loader.</para>
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is seperated and how the <paramref name="sourcePackage"/> affects package access.</para>
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
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is seperated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Card? LoadCard(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadCard(itemName);
    }

    /// <summary>
    /// Loads a <see cref="Hero"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is seperated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public Hero? LoadHero(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadHero(itemName);
    }

    /// <summary>
    /// Loads a <see cref="ActionFunction"/> from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is seperated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public ActionFunction LoadAction(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadAction(itemName) ?? new ActionFunction();
    }

    /// <summary>
    /// Loads a json file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is seperated and how the <paramref name="sourcePackage"/> affects package access.</para>
    /// </summary>
    public JToken LoadJson(string itemPath, Package? sourcePackage = null) {
        Package? package = GetPackageFromItemPath(itemPath, out string itemName, sourcePackage);
        return package?.LoadJson(itemName) ?? DeadToken.Instance;
    }

    /// <summary>
    /// Loads a <see cref="ResourceType"/> file from a package using a full or partial item path.
    /// <para>Look at <see cref="GetPackageFromItemPath(string, out string, Package?)"/> to see how <paramref name="itemPath"/> is seperated and how the <paramref name="sourcePackage"/> affects package access.</para>
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
    public bool TryGetJson(string fullItemPath, out JToken data) {
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

    /// <summary>
    /// Retrieve the name of every item in every <see cref="Package"/> that belongs to <paramref name="category"/>.
    /// <para>Each item will use package notation; including the <see cref="PackageItemSeparator"/>.</para>
    /// </summary>
    public IEnumerable<string> GetAllItemNamesInCategory(string category) {

        List<string> items = new();

        foreach (var package in Catalogue) {

            // Get the keys for each package.
            IEnumerable<string> keys = package.Value.GetAllItemNamesInCategory(category);
            // Convert the keys into the package notation with the seperator.
            IEnumerable<string> packagedKeys = keys.Select((key) => GetFullItemPath(package.Value, key));
            // Add all the keys.
            items.AddRange(packagedKeys);
        }

        return items;
    }

    public IEnumerator<Package> GetEnumerator() {
        return AllPackages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)AllPackages).GetEnumerator();
    }
}
