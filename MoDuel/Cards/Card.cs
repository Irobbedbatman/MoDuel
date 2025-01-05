using MoDuel.Abilities;
using MoDuel.Data;
using MoDuel.Resources;
using MoDuel.Shared.Json;
using System.Text.Json.Nodes;

namespace MoDuel.Cards;


/// <summary>
/// A card that has been created or loaded from a file.
/// <para>Provides access to it's <see cref="Parameters"/>, its <see cref="TriggerReactions"/> and <see cref="ExplicitTriggerReactions"/>.</para>
/// </summary>
public class Card : LoadedAsset {

    /// <summary>
    /// The type of card type. Examples are Creature or Spell.
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// The initial abilities a card will have.
    /// </summary>
    public readonly HashSet<Ability> InitialAbilities = [];

    public Card(Package package, string id, JsonObject data) : base(package, id, data) {
        Type = data["Type"]?.ToString() ?? "Creature";
    }


    /// <summary>
    /// Retrieves a value from <see cref="Data"/> called <paramref name="parameter"/>.
    /// <para>If the value is a collection access it by the <paramref name="level"/>.</para>
    /// <para>If no value is found uses the highest value in that collection.</para>
    /// </summary>
    /// <param name="parameter">The parameter to retrieve.</param>
    /// <param name="level">The level of that parameter.</param>
    /// <returns>The json data that represents the parameter value.</returns>
    public JsonNode GetLevelledParameter(string parameter, int level) {
        var parameterData = Data.Get(parameter);
        var index = parameterData.GetAdaptiveIndex(level);
        return parameterData.BaseFallbackGetOrNextLowest(index, false);
    }


}
