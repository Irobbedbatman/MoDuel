using MoDuel.Sources;

namespace MoDuel.Triggers;

/// <summary>
/// A reaction to a trigger.
/// </summary>
public class TriggerReaction {

    /// <summary>
    /// The source that created the reaction.
    /// </summary>
    public readonly SourceAbility Source;

    /// <summary>
    /// The reaction to invoke.
    /// </summary>
    public readonly ActionFunction Action;

    public TriggerReaction(SourceAbility source, ActionFunction action) {
        Source = source;
        Action = action;
    }

}
