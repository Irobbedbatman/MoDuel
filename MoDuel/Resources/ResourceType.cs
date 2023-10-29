using MoDuel.Data;
using MoDuel.Triggers;
using Newtonsoft.Json.Linq;

namespace MoDuel.Resources;

/// <summary>
/// A type for each diffrent resource. Defined by its <see cref="Name"/>. 
/// <para>Can be explicitly triggered to define custom behaviour.</para>
/// </summary>
public class ResourceType : LoadedAsset, IExplicitTriggerable {

    /// <summary>
    /// The reactions this mana type will have when requested to react to certiain trigger keywords.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> ExplicitTriggers = new();

    public ResourceType(Package package, string id, JObject data) : base(package, id, data) { }

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
