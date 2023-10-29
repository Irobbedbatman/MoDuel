using MoDuelJIT.Cards;
using MoDuelJIT.Heroes;
using MoDuelJIT.Resources;
using MoDuelJIT.Tools.JsonWrappers;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace MoDuelJIT.Data {

    /// <summary>
    /// An opened version of a <see cref="Data.Package"/> stores any assets loaded.
    /// </summary>
    public class OpenedPackage {

        /// <summary>
        /// The property in the package data that details the path to the assembly file.
        /// </summary>
        public const string ASSEMBLY_PROPERTY = "Assembly";

        /// <summary>
        /// The property in the package data that details the namespace path that the package code file is in,
        /// </summary>
        public const string NAMESPACE_PROPERTY = "Namespace";

        /// <summary>
        /// The property in the package data that details a custom name provided to the package code file.
        /// </summary>
        public const string PACKAGE_CLASS_PROPERTY = "ClassName";

        /// <summary>
        /// A category name for consistant naming conventions.
        /// <para>Users can decide to store data in other categories.</para>
        /// </summary>
        public const string
            CARD_CATEGORY = "Cards",
            HERO_CATEGORY = "Heroes",
            ACTION_CATEGORY = "Actions",
            JSON_DATA_CATEGORY = "Data",
            GLOBAL_SYSTEM_CATEGORY = "Systems",
            RESOURCE_TYPE_CATEGORY = "Resources";

        /// <summary>
        /// The <see cref="Data.Package"/> this is an instance of.
        /// </summary>
        public readonly Package Package;
        /// <summary>
        /// The name of the package transfered to the instance.
        /// </summary>
        public string PackageName => Package.Name;

        /// <summary>
        /// Gets a property from the underlying <see cref="Package"/>.
        /// </summary>
        public string? GetProperty(string propertyName) => Package.GetProperty(propertyName);

        /// <summary>
        /// Gets a path from the underlying <see cref="Package"/> using the property found under the <paramref name="propertyName"/> provided..
        /// </summary>
        public string? GetFilePropertyPath(string propertyName) => Package.GetFilePropertyPath(propertyName);

        /// <summary>
        /// The <see cref="OpenedPackageCatalogue"/> that contains this <see cref="OpenedPackageCatalogue"/>.
        /// <paa>Used to load content from the same <see cref="Package"/> easier.</paa>
        /// </summary>
        public readonly OpenedPackageCatalogue Holder;

        /// <summary>
        /// The <see cref="PackagedCode"/> that contains the compiled code of a package.
        /// </summary>
        public readonly PackagedCode PackagedCode;

        public OpenedPackage(Package package, OpenedPackageCatalogue holder) {
            Package = package;
            Holder = holder;
            // Find and setup the package code.
            // The actual loading can't be done in the constructor and is done in the Catalouge instead.
            PackagedCode = PackagedCodeFinder.FindAndInit(this);
        }

        /// <summary>
        /// Shorthand accessor for the <see cref="Package.Data"/> in the <see cref="Package"/>.
        /// </summary>
        public JTokenProxy this[string key] => Package[key];

        /// <summary>
        /// Retrieve the name of every item in the provided <paramref name="category"/>.
        /// <para>Calls the same method in the <see cref="Package"/> this object was created with.</para>
        /// </summary>
        public IEnumerable<string> GetAllItemNamesInCategory(string category) => Package.GetAllItemNamesInCategory(category);

        #region Content Loaders

        /// <summary>
        /// Loads a <see cref="Card"/> with the given id from a json file.
        /// <para>Also loads other linked content listed in the json file.</para>
        /// </summary>
        /// <param name="id">The index used to get the full path to the <see cref="Card"/> json file inside the <see cref="Package"/></param>
        /// <returns>A <see cref="Card"/> that was loaded from the json file, or the <see cref="Card"/> with same <paramref name="id"/> that had already been loaded.</returns>
        public Card? LoadCard(string id) {

            // No need to load a card that has already been loaded.
            if (LoadedCards.TryGetValue(id, out var preloadedCard))
                return preloadedCard;

            // Display a message if logging is enabled.
            if (Holder.LogLoads) {
                Console.WriteLine($"Loading Card: {id} from package {Package.Name}.");
            }

            // Get the filepath and validate it.
            string? filepath = Package.GetFullPath(CARD_CATEGORY, id);
            if (filepath == null)
                return null;

            if (!File.Exists(filepath)) {
                Console.WriteLine($"No file could be found at path: {filepath}.");
                return null;
            }

            //Convert the file to a json object.
            JObject cardData = JObject.Parse(File.ReadAllText(filepath));

            //Create the card.
            Card card = new(
                cardData["ID"]?.ToString() ?? "Card Wihtout Id",
                cardData["Type"]?.ToString() ?? "Creature",
                JTokenProxy.WrapToken(cardData["Parameters"])
            );

            // Load any content that is marked for loading.
            LoadLinkedContent(cardData["LinkedContent"]);

            // Register the card so it no longer needs to be loaded.
            LoadedCards.Add(id, card);
            // Assign the trigger reactions after loading to ensure no recusive loading.
            card.AssignTriggerReactions(GetTriggersFrom(cardData["Triggers"]), GetTriggersFrom(cardData["Explicit"]));
            return card;
        }

        /// <summary>
        /// Loads a <see cref="Hero"/> with the given id from a json file.
        /// <para>Also loads other linked content listed in the json file.</para>
        /// </summary>
        /// <param name="id">The index used to get the full path to the <see cref="Hero"/> json file inside the <see cref="Package"/></param>
        /// <returns>A <see cref="Hero"/> that was loaded from the json file, or the <see cref="Hero"/> with same <paramref name="id"/> that had already been loaded.</returns>
        public Hero? LoadHero(string id) {

            // No need to load a card that has already been loaded.
            if (LoadedHeroes.TryGetValue(id, out var preloadedHero))
                return preloadedHero;

            // Display a message if logging is enabled.
            if (Holder.LogLoads) {
                Console.WriteLine($"Loading Hero: {id} from package {Package.Name}.");
            }

            // Get the filepath and validate it.
            string? filepath = Package.GetFullPath(HERO_CATEGORY, id);
            if (filepath == null)
                return null;

            if (!File.Exists(filepath)) {
                Console.WriteLine($"No file could be found at path: {filepath}.");
                return null;
            }

            // Convert the file to a json object.
            JObject heroData = JObject.Parse(File.ReadAllText(filepath));

            // Get the static hero data.
            Hero hero = new(
                heroData?["ID"]?.ToString() ?? "Hero without id.",
                JTokenProxy.WrapToken(heroData?["Parameters"])
            );

            //Add the loaded hero to the loaded content.
            LoadedHeroes.Add(id, hero);
            // Assign the trigger reactions after loading to ensure no recusive loading.
            hero.AssignTriggerReactions(GetTriggersFrom(heroData?["Triggers"]), GetTriggersFrom(heroData?["Explicit"]));
            LoadLinkedContent(heroData?["LinkedContent"]);
            return hero;

        }

        /// <summary>
        /// Loads an action from the <see cref="PackagedCode"/> with the provided <paramref name="id"/>.
        /// </summary>
        public ActionFunction LoadAction(string id) {
            if (PackagedCode.Actions.TryGetValue(id, out var action)) {
                return action;
            }
            return new ActionFunction();
        }

        /// <summary>
        /// Loads a json object from a json file if it had not been loaed already.
        /// </summary>
        /// <param name="id">The index used to find the fullpath to the json file within the <see cref="Package"/>.</param>
        public JTokenProxy LoadJson(string id) {

            // Reuse loaded content.
            if (LoadedFiles.TryGetValue(id, out var preloadedFile))
                return JTokenProxy.WrapToken(preloadedFile);

            // Display a message if logging is enabled.
            if (Holder.LogLoads) {
                Console.WriteLine($"Loading Json File: {id} from package {Package.Name}.");
            }

            // Get the file path and validate it.
            var filepath = Package.GetFullPath(JSON_DATA_CATEGORY, id);
            if (filepath == null)
                return JTokenProxy.DeadToken;

            if (!File.Exists(filepath)) {
                Console.WriteLine($"No file could be found at path: {filepath}.");
                return JTokenProxy.DeadToken;
            }

            // Open the json data as a jobject
            var file = JObject.Parse(File.ReadAllText(filepath));
            LoadedFiles.Add(id, file);
            return JTokenProxy.WrapToken(file);
        }

        /// <summary>
        /// Loads a <see cref="ResourceType"/> from a json file.
        /// <para>Also loads any linked content. Including all the actions used as implicit/explict triggers.</para>
        /// </summary>
        /// <param name="id">The index used to find the fullpath to the resource file within the <see cref="Package"/>.</param>
        /// <returns>The loaded resource type.</returns>
        public ResourceType LoadResourceType(string id) {
            // Reuse loaded content.
            if (LoadedResourceTypes.TryGetValue(id, out var preloaded))
                return preloaded;

            if (Holder.LogLoads)
                Console.WriteLine($"Loading Resource Type: {id} from package {Package.Name}.");

            // Get the full filepath and validate it.
            var filepath = Package.GetFullPath(RESOURCE_TYPE_CATEGORY, id);
            if (filepath == null) {
                Console.WriteLine($"ResourceType: {id} not found in pakcage. Category Checked: {RESOURCE_TYPE_CATEGORY}");
                return new ResourceType("Not Found");
            }

            // Esnure the file exists.
            if (!File.Exists(filepath)) {
                Console.WriteLine($"No file could be found at path: {filepath}.");
                return new ResourceType("Not Found");
            }

            // Parse the json data.
            var jobject = JObject.Parse(File.ReadAllText(filepath));

            // Create the resource type. Provided a default name if it was not provided.
            ResourceType newType = new(jobject["Name"]?.ToString() ?? "Unamed Resource");

            // Add the resource type so that need not be reloaded.
            LoadedResourceTypes.Add(id, newType);

            // Load any explicit triggers and load them.
            newType.AssignExplicitTriggers(GetTriggersFrom(jobject["Explicit"]));

            // Load any content marked to be loaded as well.
            LoadLinkedContent(jobject["LinkedContent"]);

            return newType;

        }

        /// <summary>
        /// Goes through each category listed in <paramref name="linkedContent"/> and loads them.
        /// </summary>
        /// <param name="linkedContent">A <see cref="JObject"/> that has refernces to other content to load.</param>
        private void LoadLinkedContent(JToken? linkedContent) {

            if (linkedContent == null)
                return;

            // Get the arrays of the other content to load.
            JToken? cards = linkedContent[CARD_CATEGORY];
            JToken? heroes = linkedContent[HERO_CATEGORY];
            JToken? actions = linkedContent[ACTION_CATEGORY];
            JToken? jsonFiles = linkedContent[JSON_DATA_CATEGORY];

            // Load any requested content.
            if (cards != null)
                foreach (string c in cards.OfType<JValue>().Where((card) => card.Type == JTokenType.String).Select(card => card.ToString()))
                    Holder.LoadCard(c, this);
            if (heroes != null)
                foreach (string h in heroes.OfType<JValue>().Where((hero) => hero.Type == JTokenType.String).Select(hero => hero.ToString()))
                    Holder.LoadHero(h, this);
            if (actions != null)
                foreach (string a in actions.OfType<JValue>().Where((action) => action.Type == JTokenType.String).Select(action => action.ToString()))
                    Holder.LoadAction(a, this);
            if (jsonFiles != null)
                foreach (string j in jsonFiles.OfType<JValue>().Where((json) => json.Type == JTokenType.String).Select(json => json.ToString()))
                    Holder.LoadJson(j, this);
        }

        /// <summary>
        /// Loads all the requested trigger from a json block; linking the trigger key to a <see cref="ActionFunction"/> that will be triggered.
        /// </summary>
        /// <param name="triggerToken">A block of json with all the trigger, reaction pairs.</param>
        /// <returns>A dictionary with the keys being the triggers and the values being the reactions.</returns>
        public Dictionary<string, ActionFunction> GetTriggersFrom(JToken? triggerToken) {

            if (triggerToken == null)
                return new Dictionary<string, ActionFunction>();

            Dictionary<string, ActionFunction> triggers = new();
            foreach (JProperty trigger in triggerToken.OfType<JProperty>()) {

                // Get the trigger and the reaction.
                string triggerKey = trigger.Name;
                string triggerActionName = trigger.Value?.Value<string>() ?? "";

                // Load the reaction as an action.
                ActionFunction triggerAction = Holder.LoadAction(triggerActionName, this) ?? new ActionFunction();

                triggers.Add(triggerKey, triggerAction);
            }

            return triggers;
        }

        #endregion

        #region Content Collections

        /// <summary>
        /// All the <see cref="Card"/>s that have been loaded from their files and don't need to be reopened.
        /// </summary>
        private readonly Dictionary<string, Card> LoadedCards = new();

        /// <summary>
        /// All the <see cref="Hero"/>es that have been loaded from their files and don't need to be reopened.
        /// </summary>
        private readonly Dictionary<string, Hero> LoadedHeroes = new();

        /// <summary>
        /// All the json files that have been loaded and don't need to be reopened.
        /// </summary>
        private readonly Dictionary<string, JToken> LoadedFiles = new();

        /// <summary>
        /// All the <see cref="ResourceType"/>s that have been loaded and don't need to be reopened.
        /// </summary>
        private readonly Dictionary<string, ResourceType> LoadedResourceTypes = new();

        /// <summary>
        /// Get the amount of Loaded Items from each Dictionary Combined.
        /// <para>Could be used for debugging and getting an idea of memory usage.</para>
        /// </summary>
        public int LoadedCount => LoadedCards.Count + PackagedCode.Actions.Count + LoadedHeroes.Count + LoadedFiles.Count + LoadedResourceTypes.Count;

        /// <summary>
        /// Gets the keys of all of the loaded items from each dictionary combined.
        /// <para>Could be used to ensure that content was linked properly and other debugging situations.</para>
        /// <para>Each key comes with a prefix stating what type of content it is. Split with the character '|'.</para>
        /// </summary>
        /// <returns></returns>
        public string[] GetAllLoadedKeysSummary() {
            List<string> keys = new();
            foreach (var k in LoadedCards.Keys)
                keys.Add("Card|" + k);
            foreach (var k in PackagedCode.Actions.Keys)
                keys.Add("Action|" + k);
            foreach (var k in LoadedHeroes.Keys)
                keys.Add("Hero|" + k);
            foreach (var k in LoadedFiles.Keys)
                keys.Add("RawJson|" + k);
            foreach (var k in LoadedResourceTypes.Keys)
                keys.Add("Res|" + k);
            return keys.ToArray();
        }

        #endregion


    }
}
