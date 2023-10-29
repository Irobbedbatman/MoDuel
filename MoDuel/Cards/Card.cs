using MoDuel.Data;
using MoDuel.Json;
using Newtonsoft.Json.Linq;

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

    public Card(Package package, string id, JObject data) : base(package, id, data) {
        Type = data["Type"]?.ToString() ?? "Creature";
    }

    /// <summary>
    /// Assigns both the implicit <see cref="TriggerReactions"/> and <see cref="ExplicitTriggerReactions"/> to the <see cref="Hero"/>.
    /// </summary>
    internal Card AssignTriggerReactions(Dictionary<string, ActionFunction> triggerReactions, Dictionary<string, ActionFunction> explicitTriggerReactions) {
        // Ensure assigning only happens once.
        if (TriggerReactions.Count > 0 || ExplicitTriggerReactions.Count > 0)
            return this;
        TriggerReactions = triggerReactions ?? new();
        ExplicitTriggerReactions = explicitTriggerReactions ?? new();
        return this;
    }

    /// <summary>
    /// Retrieves a value from <see cref="Data"/> called <paramref name="parameter"/>.
    /// <para>If the value is a collection access it by the <paramref name="level"/>.</para>
    /// <para>If no value is found uses the highest value in that collection.</para>
    /// </summary>
    /// <param name="parameter">The paramter to retrieve.</param>
    /// <param name="level">The level of that paramater.</param>
    /// <returns>The json data that represents the paramater value.</returns>
    public JToken GetLeveledParameter(string parameter, int level) {
        var paramData = Data.TryGet(parameter);
        var index = paramData.GetAdaptiveIndex(level);
        return paramData.BaseFallbackGetOrNextLowest(index, false);
    }

}
