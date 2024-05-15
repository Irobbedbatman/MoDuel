using System.Collections;

namespace MoDuel.Shared.Data;

/// <summary>
/// A dictionary of <see cref="IPackage"/>s and some Properties that define how it is browsed.
/// <para>This class requires a catalogue package type pair.</para>
/// <para>There is no guarantee that the items in a package are available.</para>
/// <para>If a package is updated; it is better to create a new <see cref="BasePackageCatalogue{T}"/> so that entitles using the old one don't lose access to information.</para>
/// </summary>
/// <typeparam name="C">The Catalogue type. Must be made to hold <typeparamref name="P"/>.</typeparam>
/// <typeparam name="P">The Package type. Must be storable with in <typeparamref name="C"/>.</typeparam>
public class BasePackageCatalogue<P, C> : IEnumerable<P> where P : BasePackage<P, C> where C : BasePackageCatalogue<P, C> {

    /// <summary>
    /// Access to each <see cref="Package"/> via it's <see cref="Package.Name"/>
    /// </summary>
    protected readonly Dictionary<string, P> Packages = [];

    /// <summary>
    /// The list of all the packages stored in the catalogue.
    /// </summary>
    public IReadOnlyList<P> AllPackages => Packages.Values.ToList();

    /// <summary>
    /// The separator that separates packages from their items. I.e: package|item
    /// </summary>
    public const char PackageItemSeparator = '|';

    /// <summary>
    /// An alternative way to access the <see cref="DefaultPackage"/> instead of using it's name.
    /// </summary>
    public const string DefaultPackageIndicator = "?>";

    /// <summary>
    /// An alternative way to access packages item from the same package instead of requiring the package name. Note: this is the default behaviour when no package name is provided.
    /// </summary>
    public const string SamePackageIndicator = ">";

    /// <summary>
    /// A content package that is wil lbe used a fallback.
    /// </summary>
    public P? DefaultPackage = null;

    /// <summary>
    /// Accessor for the a <see cref="Package"/> in the catalogue.
    /// </summary>
    public P? this[string packageName] => GetPackage(packageName);

    /// <summary>
    /// Recreates the full item path of an item.
    /// <para>The item path is: <paramref name="package"/>.Name + <see cref="PackageItemSeparator"/> + <paramref name="itemName"/>.</para>
    /// </summary>
    /// <param name="package">The package the item was found in.</param>
    /// <param name="itemName">The name of the item inside the package.</param>
    /// <returns>The full path to the item.</returns>
    public static string GetFullItemPath(P package, string itemName) {
        return string.Concat(package.Name, PackageItemSeparator, itemName);
    }

    /// <summary>
    /// Retrieves the package with the provided name if it exists.
    /// </summary>
    public P? GetPackage(string packageName) => Packages.TryGetValue(packageName ?? "", out var value) ? value : null;

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
    public P? GetPackageFromItemPath(string itemPath, out string itemName, P? currentPackage = null) {

        // Separate the package name and the item name.
        var pathSplit = itemPath.Split(PackageItemSeparator);

        if (pathSplit.Length == 1) {
            // Item name is all that was provided.
            itemName = itemPath;
            // If this was not requested from a package assume the item is in the default package.
            if (currentPackage == null) {
                return DefaultPackage;
            }
            // If this was requested from a package assume the item is from that same package.
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
        if (packageName == DefaultPackageIndicator) {
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

    /// <summary>
    /// Retrieve the name of every item in every <see cref="Package"/> that belongs to <paramref name="category"/>.
    /// <para>Each item will use package notation; including the <see cref="PackageItemSeparator"/>.</para>
    /// </summary>
    public IEnumerable<string> GetAllItemNamesInCategory(string category) {

        List<string> items = [];
        foreach (var packagedKeys in from package in Packages// Get the keys for each package.
                                     let keys = package.Value.GetAllItemNamesInCategory(category) // Convert the keys into the package notation with the separator.
                                     let packagedKeys = keys.Select((key) => GetFullItemPath(package.Value, key))
                                     select packagedKeys) {
            // Add all the keys.
            items.AddRange(packagedKeys);
        }
        return items;
    }

    /// <inheritdoc/>
    public IEnumerator<P> GetEnumerator() => AllPackages.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)AllPackages).GetEnumerator();

}
