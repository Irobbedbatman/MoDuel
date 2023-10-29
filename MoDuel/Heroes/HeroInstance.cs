using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.Triggers;

namespace MoDuel.Heroes;

/// <summary>
/// An instanced version of <see cref="Hero"/> stored in a <see cref="Player"/>.
/// </summary>
[SerializeReference]
public class HeroInstance : IImplicitTriggerable, IExplicitTriggerable {

    /// <summary>
    /// The <see cref="Hero"/> this <see cref="HeroInstance"/> is taken from.
    /// </summary>
    public readonly Hero Imprint;

    /// <summary>
    /// The <see cref="Player"/> that is using this <see cref="HeroInstance"/> as their <see cref="Player.Hero"/>.
    /// </summary>
    public readonly Player Owner;

    /// <summary>
    /// Trigger reactions that are used before ones that are found in <see cref="Imprint"/>.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> NewTriggerReactions = new();
    /// <summary>
    /// Explicit trigger reactions that are used before ones that are found in <see cref="Imprint"/>.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> NewExplicitTriggerReactions = new();

    public HeroInstance(Hero hero, Player owner) {
        Imprint = hero;
        Owner = owner;
    }

    /// <summary>
    /// Add a trigger reaction to this <see cref="HeroInstance"/>.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being added an explicit trigger or an implicit tirgger.</param>
    public void AddTrigger(string triggerKey, ActionFunction triggerReaction, bool isExplicit = false) {
        if (!isExplicit)
            NewTriggerReactions.Add(triggerKey, triggerReaction);
        else
            NewExplicitTriggerReactions.Add(triggerKey, triggerReaction);
    }

    /// <summary>
    /// Adds an explicit reacion to this <see cref="HeroInstance"/>.
    /// </summary>
    public void AddExplicitTrigger(string triggerKey, ActionFunction reaction) => AddTrigger(triggerKey, reaction, true);

    /// <summary>
    /// Removes a trigger reaction.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being remvoed an explicit trigger.</param>
    public void RemoveTrigger(string triggerKey, bool isExplicit = false) {
        if (!isExplicit)
            NewTriggerReactions.Remove(triggerKey);
        else
            NewExplicitTriggerReactions.Remove(triggerKey);
    }
    /// <summary>
    /// Removes an explicit trigger reaction.
    /// </summary>
    public void RemoveExplictTrigger(string triggerKey) => NewExplicitTriggerReactions.Remove(triggerKey);

    /// <inheritdoc/>
    public ActionFunction GetExplicitReaction(string trigger) {
        // Priortise reactions on this instance first.
        if (NewExplicitTriggerReactions.TryGetValue(trigger, out var reaction))
            return reaction;
        if (Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out reaction))
            return reaction;
        return new ActionFunction();
    }
    /// <inheritdoc/>
    public ActionFunction GetImplicitReaction(string trigger) {
        // Priortise reactions on this instance first.
        if (NewTriggerReactions.TryGetValue(trigger, out var value))
            return value;
        if (Imprint.TriggerReactions.TryGetValue(trigger, out value))
            return value;
        return new ActionFunction();
    }

    /// <inheritdoc/>
    public bool HasImplicitReaction(string trigger) => NewTriggerReactions.ContainsKey(trigger) || Imprint.TriggerReactions.ContainsKey(trigger);

    /// <inheritdoc/>
    public dynamic? Trigger(string trigger, params object?[] arguments) {
        return GetExplicitReaction(trigger)?.Call(arguments.Prepend(this).ToArray());
    }

    /// <inheritdoc/>
    public dynamic? FallbackTrigger(string trigger, ActionFunction fallback, params object?[] arguments) {
        var reaction = GetExplicitReaction(trigger);
        if (reaction?.IsAssigned ?? false)
            return reaction.Call(arguments.Prepend(this).ToArray());
        return fallback?.Call(arguments.Prepend(this).ToArray());
    }

    /// <inheritdoc/>
    public dynamic? FallbackTrigger(string trigger, Delegate fallback, params object?[] arguments) {
        return FallbackTrigger(trigger, new ActionFunction(fallback), arguments);
    }
}
