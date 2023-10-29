using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.OngoingEffects;

/// <summary>
/// An effect that is made to persist as long as its active.
/// <para>Behaviour is perfromed through implict and explicit triggers.</para>
/// </summary>
[SerializeReference]
public class OngoingEffect : Target, IImplicitTriggerable, IExplicitTriggerable {

    /// <summary>
    /// The context that this <see cref="OngoingEffect"/> is running within.
    /// </summary>
    public readonly DuelState Context;

    /// <summary>
    /// Was this <see cref="OngoingEffect"/> created to be used as a global system.
    /// </summary>
    public readonly bool IsGlobal = false;

    /// <summary>
    /// A dictionary with triggers being used as keys and reactions as values.
    /// <para>The <see cref="string"/> is a trigger keyword.</para>
    /// <para>The <see cref="Closure"/> reaction is a lua function.</para>
    /// </summary>
    public readonly Dictionary<string, ActionFunction> TriggerReactions = new();

    /// <summary>
    /// A dictionary with triggers being used as keys and reactions as values.
    /// <para>Only triggers when only this object is asked to trigger..</para>
    /// <para>The <see cref="string"/> is a trigger keyword.</para>
    /// <para>The <see cref="Closure"/> reaction is a lua function.</para>
    /// </summary>
    public readonly Dictionary<string, ActionFunction> ExplicitTriggerReactions = new();

    /// <summary>
    /// The player that caused this <see cref="OngoingEffect"/> to be created.
    /// </summary>
    public readonly Player? Owner = null;

    /// <summary>
    /// The manager that will determine the activation and deactivateion of this effect.
    /// </summary>
    public readonly OngoingEffectManager Manager;

    /// <summary>
    /// Shared active state of <see cref="ExplicitTriggerActive"/> and <see cref="ImplicitTriggerActive"/>.
    /// <para>Used to simplfy disabling both.</para>
    /// </summary>
    public bool Active {
        get => ExplicitTriggerActive && ImplicitTriggerActive;
        set {
            ExplicitTriggerActive = value;
            ImplicitTriggerActive = false;
        }
    }

    /// <summary>
    /// If the explicit triggering should be used externally.
    /// </summary>
    public bool ExplicitTriggerActive = true;

    /// <summary>
    /// If the implicit triggering should be used externally
    /// </summary>
    public bool ImplicitTriggerActive = true;

    /// <summary>
    /// Creates a new ongoing effect.
    /// </summary>
    /// <param name="setActive">Wether this effect is active immeaditly.</param>
    /// <param name="player">The owner of the effect.</param>
    public OngoingEffect(DuelState context, Player? player = null, bool setActive = true) : base(context.TargetRegistry) {
        Context = context;
        Manager = context.EffectManager;
        Owner = player;
        Active = setActive;
        Register();
    }

    /// <summary>
    /// Activates this <see cref="OngoingEffect"/>; adding it to <see cref="OngoingEffects"/>.
    /// </summary>
    /// <returns>True if the effect could be activated.</returns>
    public bool Register() => Manager.RegisterOngoingEffect(this);

    /// <summary>
    /// Deactivate this <see cref="OngoingEffect"/>; removing it from <see cref="OngoingEffects"/>.
    /// </summary>
    /// <returns>True if the effect could be deactivated.</returns>
    public bool Deregister() => Manager.DeregisterOngoingEffect(this);

    /// <summary>
    /// Add a trigger reaction to this <see cref="OngoingEffect"/>.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being added an explicit trigger or an implicit tirgger.</param>
    public void AddTrigger(string triggerKey, ActionFunction triggerReaction, bool isExplicit = false) {
        if (!isExplicit)
            TriggerReactions.Add(triggerKey, triggerReaction);
        else
            ExplicitTriggerReactions.Add(triggerKey, triggerReaction);
    }

    /// <summary>
    /// Adds an explicit reaction to this <see cref="OngoingEffect"/>.
    /// </summary>
    public void AddExplicitTrigger(string triggerKey, ActionFunction reaction) => AddTrigger(triggerKey, reaction, true);

    /// <summary>
    /// Removes a trigger reaction.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being remvoed an explicit trigger.</param>
    public void RemoveTrigger(string triggerKey, bool isExplicit = false) {
        if (!isExplicit)
            TriggerReactions.Remove(triggerKey);
        else
            ExplicitTriggerReactions.Remove(triggerKey);
    }
    /// <summary>
    /// Removes an explicit trigger reaction.
    /// </summary>
    public void RemoveExplictTrigger(string triggerKey) => ExplicitTriggerReactions.Remove(triggerKey);

    /// <summary>
    /// Attermpt to get a trigger reaction from this <see cref="OngoingEffect"/>.
    /// </summary>
    public bool TryGetReaction(string trigger, out ActionFunction? value) => TriggerReactions.TryGetValue(trigger, out value);

    /// <inheritdoc/>
    public bool HasImplicitReaction(string trigger) => TriggerReactions.ContainsKey(trigger);

    /// <inheritdoc/>
    public ActionFunction GetImplicitReaction(string trigger) {
        if (TriggerReactions.TryGetValue(trigger, out var value))
            return value;
        return new ActionFunction();
    }

    /// <inheritdoc/>
    public ActionFunction GetExplicitReaction(string trigger) {
        if (ExplicitTriggerReactions.TryGetValue(trigger, out var value))
            return value;
        return new ActionFunction();
    }

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
