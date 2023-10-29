using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Json;
using MoDuel.Resources;
using Newtonsoft.Json.Linq;

namespace MoDuel.Data;


// See Package.cs for documentation.
public partial class Package {

    /// <summary>
    /// Flag that when true causes log methods to print the loaded content.
    /// </summary>
    public static bool LogLoads { get; set; } = false;

    /// <summary>
    /// Logs the <paramref name="messsage"/> if <see cref="LogLoads"/> is true.
    /// </summary>
    private static void Log(string messsage) {
        if (LogLoads) Console.WriteLine(messsage);
    }

    /// <summary>
    /// A category name for consistant naming conventions.
    /// <para>Users can decide to store data in other categories.</para>
    /// </summary>
    public const string
        CARD_CATEGORY = "Cards",
        HERO_CATEGORY = "Heroes",
        ACTION_CATEGORY = "Actions",
        JSON_DATA_CATEGORY = "Data",
        RESOURCE_TYPE_CATEGORY = "Resources";

    /// <summary>
    /// A property on data that indicates a collection of items that also need to be loaded.
    /// </summary>
    public const string DEPENDENCIES_PROPERTY = "Dependencies";

    /// <summary>
    /// The properties on data that list each trigger for that category.
    /// </summary>
    public const string
        TRIGGERS_PROPERTY = "Triggers",
        EXPLICIT_TRIGGERS_PROPERTY = "ExplicitTriggers";

    /// <summary>
    /// The code that was loaded from within the package assembly.
    /// </summary>
    public readonly PackagedCode PackagedCode;

    #region Load Methods

    /// <summary>
    /// Uses the loader for the implmented generic type.
    /// <para>This is far slower that directly using the loader.</para>
    /// </summary>
    public T? Load<T>(string id) {
        object? result = typeof(T) switch {
            Type cardType when cardType == typeof(Card) => LoadCard(id),
            Type heroType when heroType == typeof(Hero) => LoadHero(id),
            Type actionType when actionType == typeof(ActionFunction) => LoadAction(id),
            Type jsonType when jsonType.IsAssignableFrom(typeof(JToken)) => LoadJson(id),
            Type resourceType when resourceType == typeof(ResourceType) => LoadResourceType(id),
            _ => default,
        };
        return (T?)result;
    }

    /// <summary>
    /// Loads a json file for use in other loaders.
    /// <para>Use <see cref="LoadJson(string)"/> if you want to use an itemPath.</para>
    /// </summary>
    /// <param name="itemName">The name the json file will use for reloading.</param>
    /// <param name="relativePath">The relative path to the jsom file from the package. Also used as the reload key.</param>
    /// <param name="fullPath">The full system path to the json file so that it can be loaded.</param>
    /// <returns>The json data parsed or an empty <see cref="JObject"/> if there was an error.</returns>
    internal JObject LoadFile(string itemName, string? relativePath, string? fullPath) {

        // Ensure there is something to load.
        if (relativePath == null || fullPath == null) {
            Console.WriteLine($"One of the paths was missing. Relative Path: {relativePath}. FullPath: {fullPath}");
            return new JObject();
        }
        try {
            if (LoadedJsonFiles.TryGetValue(relativePath, out var file)) {
                return file;
            }
            JObject data = JObject.Parse(File.ReadAllText(fullPath));
            // Provide information to serializers on where to reload this token.
            data.SetReloadItemPath(PackageCatalogue.GetFullItemPath(this, itemName));
            LoadedJsonFiles.Add(relativePath, data);
            return data;
        }
        catch (Exception e) {
            Console.WriteLine(e.StackTrace);
            return new JObject();
        }
    }

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

        Log($"Loading Card: {id} from package {Name}.");

        if (!GetSystemPaths(CARD_CATEGORY, id, out var relativePath, out var fullPath)) {
            Console.WriteLine($"No card file could be found for id: {id}.");
            return null;
        }

        // Load the json data.
        var data = LoadFile(id, relativePath, fullPath);
        // Create the card.
        Card card = new(this, id, data);
        // Register the card so it no longer needs to be loaded.
        LoadedCards.Add(id, card);
        // Load any content that is marked for loading.
        LoadLinkedContent(data[DEPENDENCIES_PROPERTY]);
        // Assign the trigger reactions after loading to ensure no recusive loading.
        card.AssignTriggerReactions(GetTriggersFrom(data[TRIGGERS_PROPERTY]), GetTriggersFrom(data[EXPLICIT_TRIGGERS_PROPERTY]));
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
        Log($"Loading Hero: {id} from package {Name}.");

        if (!GetSystemPaths(HERO_CATEGORY, id, out var relativePath, out var fullPath)) {
            Console.WriteLine($"No hero file could be found for id: {id}.");
            return null;
        }

        // Load the json data.
        var data = LoadFile(id, relativePath, fullPath);
        // Get the static hero data.
        Hero hero = new(this, id, data);
        //Add the loaded hero to the loaded content.
        LoadedHeroes.Add(id, hero);
        // Assign the trigger reactions after loading to ensure no recusive loading.
        hero.AssignTriggerReactions(GetTriggersFrom(data[TRIGGERS_PROPERTY]), GetTriggersFrom(data[EXPLICIT_TRIGGERS_PROPERTY]));
        LoadLinkedContent(data[DEPENDENCIES_PROPERTY]);
        return hero;

    }

    /// <summary>
    /// Loads an action from the <see cref="PackagedCode"/> with the provided <paramref name="id"/>.
    /// </summary>
    public ActionFunction LoadAction(string id) {
        if (PackagedCode.Actions.TryGetValue(id, out var action)) {
            return action;
        }
        Log($"Action couldn't be found with id: {id}");
        return new ActionFunction();
    }


    /// <summary>
    /// Loads a json object from a json file if it had not been loaed already.
    /// </summary>
    /// <param name="id">The index used to find the fullpath to the json file within the <see cref="Package"/>.</param>
    public JToken LoadJson(string id) {

        // Reuse loaded content.
        if (LoadedJsonReferences.TryGetValue(id, out var preloadedFile))
            return preloadedFile;

        // Display a message if logging is enabled.
        Log($"Loading Json File: {id} from package {Name}.");

        // Get the file path and validate it.
        if (!GetSystemPaths(JSON_DATA_CATEGORY, id, out var relativePath, out var fullPath)) {
            Console.WriteLine($"No json file could be found for id: {id}.");
            return DeadToken.Instance;
        }

        // Open the json data as a jobject
        var file = LoadFile(id, relativePath, fullPath);
        LoadedJsonReferences.Add(id, file);
        return file;
    }

    /// <summary>
    /// Loads a <see cref="ResourceType"/> from a json file.
    /// <para>Also loads any linked content. Including all the actions used as implicit/explict triggers.</para>
    /// </summary>
    /// <param name="id">The index used to find the fullpath to the resource file within the <see cref="Package"/>.</param>
    /// <returns>The loaded resource type.</returns>
    public ResourceType? LoadResourceType(string id) {
        // Reuse loaded content.
        if (LoadedResourceTypes.TryGetValue(id, out var preloaded))
            return preloaded;

        Log($"Loading Resource Type: {id} from package {Name}.");

        // Get the full filepath and validate it.
        if (!GetSystemPaths(RESOURCE_TYPE_CATEGORY, id, out var relativePath, out var fullPath)) {
            Console.WriteLine($"ResourceType: {id} not found in package.");
            return null;
        }

        // Load the json data from the file.
        var data = LoadFile(id, relativePath, fullPath);

        // Create the resource type. Provided a default name if it was not provided.
        ResourceType newType = new(this, id, data);

        // Add the resource type so that need not be reloaded.
        LoadedResourceTypes.Add(id, newType);

        // Load any explicit triggers and load them.
        newType.AssignExplicitTriggers(GetTriggersFrom(data[EXPLICIT_TRIGGERS_PROPERTY]));

        // Load any content marked to be loaded as well.
        LoadLinkedContent(data[DEPENDENCIES_PROPERTY]);

        return newType;

    }

    #endregion

    #region Helper Methods

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
        JToken? resourceFiles = linkedContent[RESOURCE_TYPE_CATEGORY];

        // Load any requested content.
        if (cards != null)
            foreach (string c in cards.OfType<JValue>().Where((card) => card.Type == JTokenType.String).Select(card => card.ToString()))
                Catalogue.LoadCard(c, this);
        if (heroes != null)
            foreach (string h in heroes.OfType<JValue>().Where((hero) => hero.Type == JTokenType.String).Select(hero => hero.ToString()))
                Catalogue.LoadHero(h, this);
        if (actions != null)
            foreach (string a in actions.OfType<JValue>().Where((action) => action.Type == JTokenType.String).Select(action => action.ToString()))
                Catalogue.LoadAction(a, this);
        if (jsonFiles != null)
            foreach (string j in jsonFiles.OfType<JValue>().Where((json) => json.Type == JTokenType.String).Select(json => json.ToString()))
                Catalogue.LoadJson(j, this);
        if (resourceFiles != null)
            foreach (string j in resourceFiles.OfType<JValue>().Where((json) => json.Type == JTokenType.String).Select(json => json.ToString()))
                Catalogue.LoadResourceType(j, this);
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

            ActionFunction triggerAction;

            // Load the reaction as an action.
            triggerAction = Catalogue.LoadAction(triggerActionName, this) ?? new ActionFunction();

            triggers.Add(triggerKey, triggerAction);
        }

        return triggers;
    }

    #endregion

    #region Content Collections

    /// <summary>
    /// Returns a set that contains all loaded <see cref="Card"/>s.
    /// </summary>
    public IEnumerable<Card> GetLoadedCards() => LoadedCards.Values;
    /// <summary>
    /// Returns a set that contains all loaded <see cref="Hero"/>es.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Hero> GetLoadedHeroes() => LoadedHeroes.Values;
    /// <summary>
    /// Returns a set that contains all loaded json files.
    /// </summary>
    public IEnumerable<JToken> GetLoadedJson() => LoadedJsonReferences.Values;
    /// <summary>
    /// Returns a set that contains all loaded <see cref="ActionFunction"/>s.
    /// </summary>
    public IEnumerable<ActionFunction> GetLoadedActions() => PackagedCode.Actions.Values;
    /// <summary>
    /// Returns a set that contains all loaded <see cref="ResourceType"/>s.
    /// </summary>
    public IEnumerable<ResourceType> GetLoadedResourceTypes() => LoadedResourceTypes.Values;

    /// <summary>
    /// All the <see cref="Card"/>s that have been loaded from their files and don't need to be reopened.
    /// </summary>
    public readonly Dictionary<string, Card> LoadedCards = new();

    /// <summary>
    /// All the <see cref="Hero"/>es that have been loaded from their files and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, Hero> LoadedHeroes = new();

    /// <summary>
    /// All the json files that have been loaded by a key and dont need to be reopned.
    /// </summary>
    private readonly Dictionary<string, JToken> LoadedJsonReferences = new();

    /// <summary>
    /// All the raw json files and their paths that have been loaded and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, JObject> LoadedJsonFiles = new();

    /// <summary>
    /// All the <see cref="ResourceType"/>s that have been loaded and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, ResourceType> LoadedResourceTypes = new();

    /// <summary>
    /// Get the amount of Loaded Items from each Dictionary Combined.
    /// <para>Could be used for debugging and getting an idea of memory usage.</para>
    /// </summary>
    public int LoadedCount => LoadedCards.Count + PackagedCode.Actions.Count + LoadedHeroes.Count + LoadedJsonReferences.Count + LoadedResourceTypes.Count;

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
        foreach (var k in LoadedJsonReferences.Keys)
            keys.Add("Json|" + k);
        foreach (var k in LoadedResourceTypes.Keys)
            keys.Add("Res|" + k);
        return keys.ToArray();
    }

    #endregion
}
