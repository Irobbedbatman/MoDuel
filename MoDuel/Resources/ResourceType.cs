using MoDuel.Data;
using MoDuel.Triggers;
using System.Text.Json.Nodes;

namespace MoDuel.Resources;

/// <summary>
/// A type for each different resource. Defined by its <see cref="Name"/>. 
/// <para>Can be explicitly triggered to define custom behaviour.</para>
/// </summary>
public class ResourceType(Package package, string id, JsonObject data) : LoadedAsset(package, id, data), IExplicitTriggerable {

    /// <summary>
    /// The reactions this mana type will have when requested to react to certain trigger keywords.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> ExplicitTriggers = [];

    /// <summary>
    /// Assign all the provided <paramref name="exTriggers"/> within <see cref="ExplicitTriggers"/>.
    /// </summary>
    internal void AssignExplicitTriggers(Dictionary<string, ActionFunction> exTriggers) {
        if (exTriggers == null)
            return;
        foreach (var exTrigger in exTriggers) {
            ExplicitTriggers.Add(exTrigger.Key, exTrigger.Value);
        }
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <inheritdoc/>
    public ActionFunction GetExplicitReaction(string trigger) {
        return ExplicitTriggers.TryGetValue(trigger, out var reaction) ? reaction : new ActionFunction();
    }

    /// <inheritdoc/>
    public dynamic? Trigger(string trigger, object?[] arguments) {
        return GetExplicitReaction(trigger)?.Call(arguments.Prepend(this).ToArray());
    }

    /// <inheritdoc/>
    public dynamic? FallbackTrigger(string trigger, ActionFunction fallback, params object?[] arguments) {
        var reaction = GetExplicitReaction(trigger);
        if (reaction.IsAssigned)
            return reaction.Call(arguments.Prepend(this).ToArray());
        return fallback?.Call(arguments.Prepend(this).ToArray());
    }

    /// <inheritdoc/>
    public dynamic? FallbackTrigger(string trigger, Delegate fallback, params object?[] arguments) {
        return FallbackTrigger(trigger, new ActionFunction(fallback), arguments);
    }

}
