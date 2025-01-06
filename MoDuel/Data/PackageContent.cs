using MoDuel.Abilities;
using MoDuel.Cards;
using MoDuel.Data.Assembled;
using MoDuel.Heroes;
using MoDuel.Resources;
using MoDuel.Shared.Json;
using MoDuel.Sources;
using System.Text.Json.Nodes;

namespace MoDuel.Data;

// See Package.cs for documentation.
public partial class Package {

    /// <summary>
    /// Logs the <paramref name="message"/> if <see cref="LogLoads"/> is true.
    /// </summary>
    private static void LogLoad(string message) => LogSettings.LogEvent(message, LogSettings.LogEvents.DataLoading);

    /// <summary>
    /// A category name for consistent naming conventions.
    /// <para>Users can decide to store data in other categories.</para>
    /// </summary>
    public const string
        CARD_CATEGORY = "Cards",
        HERO_CATEGORY = "Heroes",
        ACTION_CATEGORY = "Actions",
        ABILITY_CATEGORY = "Abilities",
        JSON_DATA_CATEGORY = "Data",
        RESOURCE_TYPE_CATEGORY = "Resources";

    /// <summary>
    /// A property on data that indicates a collection of items that also need to be loaded.
    /// </summary>
    public const string DEPENDENCIES_PROPERTY = "Dependencies";

    /// <summary>
    /// A property on data that indicates a collection of ability references..
    /// </summary>
    public const string ABILITIES_PROPERTY = ABILITY_CATEGORY;

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

    /// <inheritdoc/>
    public override bool HasItem(string category, string itemName) {

        // Need to account for actions and abilities, these are preloaded.
        if (category == ACTION_CATEGORY) {
            return PackagedCode.Actions.ContainsKey(itemName);
        }
        if (category == ABILITY_CATEGORY) {
            return PackagedCode.Abilities.ContainsKey(itemName);
        }

        return base.HasItem(category, itemName);
    }

    #region Load Methods

    /// <summary>
    /// Uses the loader for the implemented generic type.
    /// <para>This is far slower that directly using the loader.</para>
    /// </summary>
    public T? Load<T>(string id) {
        object? result = typeof(T) switch {
            Type cardType when cardType == typeof(Card) => LoadCard(id),
            Type heroType when heroType == typeof(Hero) => LoadHero(id),
            Type actionType when actionType == typeof(ActionFunction) => LoadAction(id),
            Type jsonType when jsonType.IsAssignableFrom(typeof(JsonNode)) => LoadJson(id),
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
    /// <param name="relativePath">The relative path to the json file from the package. Also used as the reload key.</param>
    /// <param name="fullPath">The full system path to the json file so that it can be loaded.</param>
    /// <returns>The json data parsed or an empty <see cref="JObject"/> if there was an error.</returns>
    internal JsonObject LoadFile(string itemName, string? relativePath, string? fullPath) {

        // Ensure there is something to load.
        if (relativePath == null || fullPath == null) {
            Console.WriteLine($"One of the paths was missing. Relative Path: {relativePath}. FullPath: {fullPath}");
            return [];
        }
        try {
            if (LoadedJsonFiles.TryGetValue(relativePath, out var file)) {
                return file;
            }
            JsonObject data = CreateJson.FromFile(fullPath);
            // Provide information to serializers on where to reload this token.
            data.SetReloadItemPath(PackageCatalogue.GetFullItemPath(this, itemName));
            LoadedJsonFiles.Add(relativePath, data);
            return data;
        }
        catch (Exception e) {
            LogSettings.LogEvent(e.StackTrace ?? "", LogSettings.LogEvents.DataLoadingError);
            return [];
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

        LogLoad($"Loading Card: {id} from package {Name}.");

        if (!GetSystemPaths(CARD_CATEGORY, id, out var relativePath, out var fullPath)) {
            LogSettings.LogEvent($"No card file could be found for id: {id}.", LogSettings.LogEvents.DataLoadingError);
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

        // Add all the abilities.
        var abilities = LoadListedAbilities(data[ABILITIES_PROPERTY]);
        foreach (var ability in abilities) {
            card.InitialAbilities.Add(ability);
        }

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
        LogLoad($"Loading Hero: {id} from package {Name}.");

        if (!GetSystemPaths(HERO_CATEGORY, id, out var relativePath, out var fullPath)) {
            LogSettings.LogEvent($"No hero file could be found for id: {id}.", LogSettings.LogEvents.DataLoadingError);
            return null;
        }

        // Load the json data.
        var data = LoadFile(id, relativePath, fullPath);
        // Get the static hero data.
        Hero hero = new(this, id, data);
        //Add the loaded hero to the loaded content.
        LoadedHeroes.Add(id, hero);

        // Add all the abilities.
        var abilities = LoadListedAbilities(data[ABILITIES_PROPERTY]);
        foreach (var ability in abilities) {
            hero.InitialAbilities.Add(ability);
        }

        // Assign the trigger reactions after loading to ensure no recursive loading.
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
        LogLoad($"Action couldn't be found with id: {id}");
        return new ActionFunction();
    }

    /// <summary>
    /// Loads an ability from the <see cref="PackagedCode"/> with the provided <paramref name="id"/>.
    /// </summary>
    public Ability LoadAbility(string id) {
        if (PackagedCode.Abilities.TryGetValue(id, out var ability)) {
            return ability;
        }
        LogLoad($"Ability couldn't be found with id: {id}");
        return Ability.Missing;
    }


    /// <summary>
    /// Loads a json object from a json file if it had not been loaded already.
    /// </summary>
    /// <param name="id">The index used to find the full path to the json file within the <see cref="Package"/>.</param>
    public JsonNode LoadJson(string id) {

        // Reuse loaded content.
        if (LoadedJsonReferences.TryGetValue(id, out var preloadedFile))
            return preloadedFile;

        // Display a message if logging is enabled.
        LogLoad($"Loading Json File: {id} from package {Name}.");

        // Get the file path and validate it.
        if (!GetSystemPaths(JSON_DATA_CATEGORY, id, out var relativePath, out var fullPath)) {
            LogSettings.LogEvent($"No json file could be found for id: {id}.", LogSettings.LogEvents.DataLoadingError);
            return DeadToken.Instance;
        }

        // Open the json data as a json object
        var file = LoadFile(id, relativePath, fullPath);
        LoadedJsonReferences.Add(id, file);
        return file;
    }

    /// <summary>
    /// Loads a <see cref="ResourceType"/> from a json file.
    /// <para>Also loads any linked content. Including all the actions used as implicit/explicit triggers.</para>
    /// </summary>
    /// <param name="id">The index used to find the full path to the resource file within the <see cref="Package"/>.</param>
    /// <returns>The loaded resource type.</returns>
    public ResourceType? LoadResourceType(string id) {
        // Reuse loaded content.
        if (LoadedResourceTypes.TryGetValue(id, out var preloaded))
            return preloaded;

        LogLoad($"Loading Resource Type: {id} from package {Name}.");

        // Get the full file path and validate it.
        if (!GetSystemPaths(RESOURCE_TYPE_CATEGORY, id, out var relativePath, out var fullPath)) {
            LogSettings.LogEvent($"ResourceType: {id} not found in package.", LogSettings.LogEvents.DataLoadingError);
            return null;
        }

        // Load the json data from the file.
        var data = LoadFile(id, relativePath, fullPath);

        // Create the resource type. Provided a default name if it was not provided.
        ResourceType newType = new(this, id, data);

        // Add the resource type so that need not be reloaded.
        LoadedResourceTypes.Add(id, newType);

        // Load all the abilities for the resource type.
        var abilities = LoadListedAbilities(data[ABILITIES_PROPERTY]);
        newType.Abilities.AddRange(abilities.Select(a => new AbilityReference(newType, new SourceImprint(newType), a)));

        // Load any content marked to be loaded as well.
        LoadLinkedContent(data[DEPENDENCIES_PROPERTY]);

        return newType;

    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Goes through the provided <paramref name="abilitiesToken"/> and loads every ability referenced.
    /// </summary>
    /// <returns>All the loaded abilities mentioned in the token.</returns>
    private List<Ability> LoadListedAbilities(JsonNode? abilitiesToken) {

        if (abilitiesToken == null)
            return [];

        var abilities = new List<Ability>();

        // Get all the ability from the token and load them.
        var abilityKeys = abilitiesToken.GetValues().Select(a => a.GetValue<string>()).OfType<string>();
        foreach (var abilityKey in abilityKeys) {
            abilities.Add(Catalogue.LoadAbility(abilityKey, this));
        }

        return abilities;
    }

    /// <summary>
    /// Goes through each category listed in <paramref name="linkedContent"/> and loads them.
    /// </summary>
    /// <param name="linkedContent">A <see cref="JObject"/> that has references to other content to load.</param>
    private void LoadLinkedContent(JsonNode? linkedContent) {

        if (linkedContent == null)
            return;

        // Get the arrays of the other content to load.
        JsonNode? cards = linkedContent[CARD_CATEGORY];
        JsonNode? heroes = linkedContent[HERO_CATEGORY];
        JsonNode? actions = linkedContent[ACTION_CATEGORY];
        JsonNode? abilities = linkedContent[ABILITY_CATEGORY];
        JsonNode? jsonFiles = linkedContent[JSON_DATA_CATEGORY];
        JsonNode? resourceFiles = linkedContent[RESOURCE_TYPE_CATEGORY];

        // Load any requested content.
        if (cards != null)
            foreach (string c in cards.GetValues().Select(card => card.GetValue<string>()).Where(card => card != null))
                Catalogue.LoadCard(c, this);
        if (heroes != null)
            foreach (string h in heroes.GetValues().Select((hero) => hero.GetValue<string>()).Where(hero => hero != null))
                Catalogue.LoadHero(h, this);
        if (actions != null)
            foreach (string a in actions.GetValues().Select((action) => action.GetValue<string>()).Where(action => action != null))
                Catalogue.LoadAction(a, this);
        if (abilities != null) {
            foreach (string a in abilities.GetValues().Select((ability) => ability.GetValue<string>()).Where(ability => ability != null))
                Catalogue.LoadAbility(a, this);
        }
        if (jsonFiles != null)
            foreach (string j in jsonFiles.GetValues().Select((json) => json.GetValue<string>()).Where(json => json != null))
                Catalogue.LoadJson(j, this);
        if (resourceFiles != null)
            foreach (string j in resourceFiles.GetValues().Select((resource) => resource.GetValue<string>()).Where(resource => resource != null))
                Catalogue.LoadResourceType(j, this);
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
    public IEnumerable<JsonNode> GetLoadedJson() => LoadedJsonReferences.Values;
    /// <summary>
    /// Returns a set that contains all loaded <see cref="ActionFunction"/>s.
    /// </summary>
    public IEnumerable<ActionFunction> GetLoadedActions() => PackagedCode.Actions.Values;
    /// <summary>
    /// Returns a set that contains all loaded <see cref="ResourceType"/>s.
    /// </summary>
    public IEnumerable<ResourceType> GetLoadedResourceTypes() => LoadedResourceTypes.Values;
    /// <summary>
    /// Returns a set that contains all loaded <see cref="Ability"/>s.
    /// </summary>
    public IEnumerable<Ability> GetLoadedAbilities() => PackagedCode.Abilities.Values;

    /// <summary>
    /// All the <see cref="Card"/>s that have been loaded from their files and don't need to be reopened.
    /// </summary>
    public readonly Dictionary<string, Card> LoadedCards = [];

    /// <summary>
    /// All the <see cref="Hero"/>es that have been loaded from their files and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, Hero> LoadedHeroes = [];

    /// <summary>
    /// All the json files that have been loaded by a key and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, JsonNode> LoadedJsonReferences = [];

    /// <summary>
    /// All the raw json files and their paths that have been loaded and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, JsonObject> LoadedJsonFiles = [];

    /// <summary>
    /// All the <see cref="ResourceType"/>s that have been loaded and don't need to be reopened.
    /// </summary>
    private readonly Dictionary<string, ResourceType> LoadedResourceTypes = [];

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
        List<string> keys = [];
        foreach (var k in LoadedCards.Keys)
            keys.Add("Card|" + k);
        foreach (var k in PackagedCode.Actions.Keys)
            keys.Add("Action|" + k);
        foreach (var k in PackagedCode.Abilities.Keys)
            keys.Add("Ability|" + k);
        foreach (var k in LoadedHeroes.Keys)
            keys.Add("Hero|" + k);
        foreach (var k in LoadedJsonReferences.Keys)
            keys.Add("Json|" + k);
        foreach (var k in LoadedResourceTypes.Keys)
            keys.Add("Res|" + k);
        return [.. keys];
    }

    #endregion
}
