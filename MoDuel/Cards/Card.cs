using MoDuel.Data;
using MoDuel.Json;
using System.Text.Json.Nodes;

namespace MoDuel.Cards;


/// <summary>
/// A card that has been created or loaded from a file.
/// <para>Provides access to it's <see cref="Parameters"/>, its <see cref="TriggerReactions"/> and <see cref="ExplicitTriggerReactions"/>.</para>
/// </summary>
public class Card(Package package, string id, JsonObject data) : LoadedAsset(package, id, data) {

    /// <summary>
    /// The type of card type. Examples are Creature or Spell.
    /// </summary>
    public readonly string Type = data["Type"]?.ToString() ?? "Creature";

    /// <summary>
    /// A dictionary with triggers being used as keys and reactions as values.
    /// <para>The <see cref="string"/> is a trigger keyword.</para>
    /// <para>The <see cref="ActionFunction"/> is the reaction.</para>
    /// </summary>
    public IReadOnlyDictionary<string, ActionFunction> TriggerReactions { get; private set; } = new Dictionary<string, ActionFunction>();

    /// <summary>
    /// A dictionary with triggers being used as keys and reactions as values.
    /// <para>Only triggers when only this object is asked to trigger..</para>
    /// <para>The <see cref="string"/> is a trigger keyword.</para>
    /// <para>The <see cref="ActionFunction"/> is the reaction.</para>
    /// </summary>
    public IReadOnlyDictionary<string, ActionFunction> ExplicitTriggerReactions { get; private set; } = new Dictionary<string, ActionFunction>();

    /// <summary>
    /// Assigns both the implicit <see cref="TriggerReactions"/> and <see cref="ExplicitTriggerReactions"/> to the <see cref="Hero"/>.
    /// </summary>
    internal Card AssignTriggerReactions(Dictionary<string, ActionFunction> triggerReactions, Dictionary<string, ActionFunction> explicitTriggerReactions) {
        // Ensure assigning only happens once.
        if (TriggerReactions.Count > 0 || ExplicitTriggerReactions.Count > 0)
            return this;
        TriggerReactions = triggerReactions ?? [];
        ExplicitTriggerReactions = explicitTriggerReactions ?? [];
        return this;
    }

    /// <summary>
    /// Retrieves a value from <see cref="Data"/> called <paramref name="parameter"/>.
    /// <para>If the value is a collection access it by the <paramref name="level"/>.</para>
    /// <para>If no value is found uses the highest value in that collection.</para>
    /// </summary>
    /// <param name="parameter">The parameter to retrieve.</param>
    /// <param name="level">The level of that parameter.</param>
    /// <returns>The json data that represents the parameter value.</returns>
    public JsonNode GetLeveledParameter(string parameter, int level) {
        var paramData = Data.Get(parameter);
        var index = paramData.GetAdaptiveIndex(level);
        return paramData.BaseFallbackGetOrNextLowest(index, false);
    }

}
