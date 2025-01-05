using MoDuel.Sources;

namespace MoDuel.Triggers;

/// <summary>
/// A source that references by a <see cref="TriggerReaction"/>.
/// </summary>
public class TriggerReactionSource : Source {

    /// <summary>
    /// The reaction that created the source.
    /// </summary>
    public readonly TriggerReaction Component;

    public TriggerReactionSource(TriggerReaction component) {
        Component = component;
    }
}
