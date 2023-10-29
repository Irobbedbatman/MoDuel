using MoDuelJIT.Cards;
using MoDuelJIT.Heroes;
using MoDuelJIT.OngoingEffects;
using MoDuelJIT.Resources;
using MoDuelJIT.Tools;
using MoDuelJIT.Tools.JsonWrappers;
using Newtonsoft.Json.Linq;

namespace MoDuelJIT.Data {

    /// <summary>
    /// An opened version of a <see cref="PackageCatalogue"/>.
    /// <para>Stores loaded content inside <see cref="OpenedPackages"/>.</para>
    /// <para>Provides <see cref="OpenedPackages"/> with support for inside package shorthand loading.</para>
    /// </summary>
    public class OpenedPackageCatalogue {

        /// <summary>
        /// All the <see cref="Package"/>s that have been opened. Uses the <see cref="Package"/> as a key.
        /// </summary>
        public readonly Dictionary<Package, OpenedPackage> OpenedPackages = new();
        /// <summary>
        /// The catalogue that records all the packages avalible and how to opene them.
        /// </summary>
        public readonly PackageCatalogue Catalogue;
        /// <summary>
        /// The execution environment of any code.
        /// </summary>
        /// <summary>
        /// Wether each time a new load request is made through a <see cref="OpenedPackage"/> a message will be displayed.
        /// <para>Won't display a message when the requested item was already loaded.</para>
        /// </summary>
        public bool LogLoads = false;

        public OpenedPackageCatalogue(PackageCatalogue catalogue) {
            Catalogue = catalogue;
        }

        /// <summary>
        /// Gets a <see cref="OpenedPackage"/> of the provided <see cref="Package"/>.
        /// <para>If it has already been opened return the opened instance.</para>
        /// </summary>
        public OpenedPackage? OpenPackage(Package? package) {
            if (package == null)
                return null;
            // Check to see if the package has already been opened.
            if (OpenedPackages.TryGetValue(package, out var instance)) {
                return instance;
            }
            // Open the package.
            OpenedPackage? newInstance = new(package, this);
            // Record the opened package so that it doesnt need to be reopened.
            OpenedPackages.Add(package, newInstance);
            // Load the code from the opened package. Done here to resolve recursive operations.
            newInstance.PackagedCode.Load();
            return newInstance;
        }

        /// <summary>
        /// Gets the <see cref="OpenedPackage"/> that the <paramref name="itemName"/> is found in.
        /// <para>Look at <see cref="PackageCatalogue"/> to see how <paramref name="itemPath"/> is defined.</para>
        /// <para>Cotent requested from the same package is priortised.</para>
        /// </summary>
        /// <param name="itemPath">A path that cotnains both the packages name and the <paramref name="itemName"/>. Can also be just the <paramref name="itemName"/> if loading a relative item.</param>
        /// <param name="itemName">The name of the item requested.</param>
        /// <param name="currentlyOpenedPackage">The package used when loading item's ambigously; these typically have prefernce over the default directory.</param>
        /// <returns>The <see cref="OpenedPackage"/> that will contain the item.</returns>
        public OpenedPackage? GetPackageFromItem(string itemPath, out string itemName, OpenedPackage? currentlyOpenedPackage = null) {
            var package = Catalogue.GetPackageFromItemPath(itemPath, out itemName, currentlyOpenedPackage?.Package ?? null);
            return OpenPackage(package);
        }

        /// <summary>
        /// Loads a card file from a package.
        /// <para>Look at <see cref="PackageCatalogue"/> to see how <paramref name="fullItemPath"/> is sperated and how the <paramref name="currentlyOpenedPackage"/> affects package access.</para>
        /// </summary>
        public Card? LoadCard(string fullItemPath, OpenedPackage? currentlyOpenedPackage = null) {
            OpenedPackage? package = GetPackageFromItem(fullItemPath, out string itemName, currentlyOpenedPackage);
            return package?.LoadCard(itemName);
        }

        /// <summary>
        /// Loads a hero file from a package.
        /// <para>Look at <see cref="PackageCatalogue"/> to see how <paramref name="fullItemPath"/> is sperated and how the <paramref name="currentlyOpenedPackage"/> affects package access.</para>
        /// </summary>
        public Hero? LoadHero(string fullItemPath, OpenedPackage? currentlyOpenedPackage = null) {
            OpenedPackage? package = GetPackageFromItem(fullItemPath, out string itemName, currentlyOpenedPackage);
            return package?.LoadHero(itemName);
        }

        /// <summary>
        /// Loads a action file from a package.
        /// <para>Look at <see cref="PackageCatalogue"/> to see how <paramref name="fullItemPath"/> is sperated and how the <paramref name="currentlyOpenedPackage"/> affects package access.</para>
        /// </summary>
        public ActionFunction? LoadAction(string fullItemPath, OpenedPackage? currentlyOpenedPackage = null) {
            OpenedPackage? package = GetPackageFromItem(fullItemPath, out string itemName, currentlyOpenedPackage);
            return package?.LoadAction(itemName);
        }

        /// <summary>
        /// Loads a json data file from a package.
        /// <para>Look at <see cref="PackageCatalogue"/> to see how <paramref name="fullItemPath"/> is sperated and how the <paramref name="currentlyOpenedPackage"/> affects package access.</para>
        /// </summary>
        public JTokenProxy LoadJson(string fullItemPath, OpenedPackage? currentlyOpenedPackage = null) {
            OpenedPackage? package = GetPackageFromItem(fullItemPath, out string itemName, currentlyOpenedPackage);
            return package?.LoadJson(itemName) ?? JTokenProxy.DeadToken;
        }

        /// <summary>
        /// Loads a <see cref="ResourceType"/> from a package.
        /// <para>Look at <see cref="PackageCatalogue"/> to see how <paramref name="fullItemPath"/> is sperated and how the <paramref name="currentlyOpenedPackage"/> affects package access.</para>
        /// </summary>
        public ResourceType? LoadResourceType(string fullItemPath, OpenedPackage? currentlyOpenedPackage = null) {
            OpenedPackage? package = GetPackageFromItem(fullItemPath, out string itemName, currentlyOpenedPackage);
            return package?.LoadResourceType(itemName);
        }

        /// <summary>
        /// Opens the <see cref="Package"/> with the provided <paramref name="packageName"/>.
        /// <para><paramref name="_"/> is provided as argument for future proofing. One example for requiring this would be subpackages.</para>
        /// </summary>
        public OpenedPackage? GetPackage(string packageName, OpenedPackage? _ = null) {
            Package? package = Catalogue.GetPackage(packageName);
            return OpenPackage(package);
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
            return action != null;
        }

        /// <summary>
        /// Try to load a json object stored in a file.
        /// </summary>
        /// <returns><c>true</c> if the json file was able to be loaded.</returns>
        public bool TryGetJson(string fullItemPath, out JTokenProxy data) {
            data = LoadJson(fullItemPath);
            return data != null;
        }

        /// <summary>
        /// Try to load a <see cref="ResourceType"/> stored in a file.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="ResourceType"/> was loaded.</returns>
        public bool TryLoadResourceType(string fullItemPath, out ResourceType? resourceType) {
            resourceType = LoadResourceType(fullItemPath);
            return resourceType != null;
        }

        /// <summary>
        /// Retrieve the name of every item in every <see cref="Package"/> that belongs to <paramref name="category"/>.
        /// <para>Calls the same method in the <see cref="Catalogue"/> this object was created with.</para>
        /// </summary>
        public IEnumerable<string> GetAllItemNamesInCategory(string category) => Catalogue.GetAllItemNamesInCategory(category);

        /// <summary>
        /// Seaches each package for any listed global systems and returns it's <see cref="GlobalSystemInformation"/>.
        /// </summary>
        public IEnumerable<GlobalEffectInformation> GetAllGlobalSystems() {

            // The list of all global systems that will were found.
            List<GlobalEffectInformation> result = new();

            // Get the name of every single global system so that they can all be loaded.
            var items = GetAllItemNamesInCategory(OpenedPackage.GLOBAL_SYSTEM_CATEGORY);

            foreach (var item in items) {

                // Get the package the item is contained within. This package is required to load actions later.
                OpenedPackage? package = GetPackageFromItem(item, out string itemName);

                if (package == null)
                    continue;

                if (LogLoads) {
                    Console.WriteLine($"Loading Global System: {itemName} from package {package.PackageName}.");
                }

                // Get the full path to the system's json data.
                var path = package.Package.GetFullPath(OpenedPackage.GLOBAL_SYSTEM_CATEGORY, itemName);

                // Ensure the file exists.
                if (!File.Exists(path)) {
                    if (LogLoads)
                        Console.WriteLine($"File countnot be found at: {path}");
                    continue;
                }

                // The json data that will be loaded from a file.
                JObject block;
                try {
                    block = JObject.Parse(File.ReadAllText(path));
                }
                catch {
                    Console.WriteLine($"Failed to load json file at: {path}");
                    continue;
                }

                // Retrieve the data stored with in the json and fill in the information.
                GlobalEffectInformation globalSystem = new(
                    block["ID"]?.ToString() ?? "",
                    block["EachPlayer"]?.Value<bool>() ?? false,
                    package.GetTriggersFrom(block["Triggers"]),
                    package.GetTriggersFrom(block["Explicit"])
                );

                // Add the found system.
                result.Add(globalSystem);
            }
            return result;
        }

    }
}
