using MoDuel.Sources;
using MoDuel.State;

namespace MoDuel.Triggers;

/// <summary>
/// A trigger that has been invoked.
/// </summary>
public class Trigger {

    /// <summary>
    /// The trigger key that listeners react to.
    /// </summary>
    public readonly string Key;

    /// <summary>
    /// The <see cref="Source"/> that created the trigger.
    /// </summary>
    public readonly Source Source;

    /// <summary>
    /// The state the trigger was executed in.
    /// </summary>
    public readonly DuelState State;

    /// <summary>
    /// The type of <see cref="Trigger"/> this.
    /// </summary>
    public readonly TriggerType TriggerType;

    /// <summary>
    /// An optional trigger this stems from.
    /// </summary>
    public readonly Trigger? ParentTrigger;

    public Trigger(string key, Source source, DuelState state, TriggerType triggerType, Trigger? parentTrigger = null) {
        Key = key;
        Source = source;
        State = state;
        TriggerType = triggerType;
        ParentTrigger = parentTrigger;
    }


    /// <summary>
    /// Get the reactions to this trigger via the state.
    /// </summary>
    public IEnumerable<TriggerReaction> GetReactions() {
        return State.GetReactions(this);
    }

    /// <summary>
    /// Create a trigger stemming from this trigger that is used to override the data retrieved from this trigger.
    /// </summary>
    public Trigger CreateSubOverrideTrigger(TriggerReaction reaction) {
        return new Trigger("Override", new TriggerReactionSource(reaction), State, TriggerType.DataOverride, this);
    }

    /// <summary>
    /// Create a trigger stemming from this trigger that validates whether a data change caused by this trigger should occur.
    /// </summary>
    public Trigger CreateSubValidationTrigger(TriggerReaction reaction) {
        return new Trigger("Validation", new TriggerReactionSource(reaction), State, TriggerType.Validation, this);
    }

    /// <summary>
    /// Create a trigger stemming from this trigger that is used to override the data explicitly retrieved from this trigger.
    /// </summary>
    public Trigger CreateSubExplicitOverrideTrigger() {
        return new Trigger("ExplicitOverride", Source, State, TriggerType.ExplicitDataOverride, this);
    }


}
