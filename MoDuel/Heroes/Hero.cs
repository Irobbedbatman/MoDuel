using MoDuel.Data;
using System.Text.Json.Nodes;

namespace MoDuel.Heroes;

/// <summary>
/// A <see cref="Hero"/> that has been created or loaded from a file.
/// <para>Provides access to it's <see cref="Parameters"/>, its <see cref="TriggerReactions"/> and <see cref="ExplicitTriggerReactions"/>.</para>
/// </summary>
public class Hero(Package package, string id, JsonObject data) : LoadedAsset(package, id, data) {
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
    internal Hero AssignTriggerReactions(Dictionary<string, ActionFunction> triggerReactions, Dictionary<string, ActionFunction> explicitTriggerReactions) {
        // Ensure assigning only happens once.
        if (TriggerReactions.Count > 0 || ExplicitTriggerReactions.Count > 0)
            return this;
        TriggerReactions = triggerReactions ?? [];
        ExplicitTriggerReactions = explicitTriggerReactions ?? [];
        return this;
    }

}
