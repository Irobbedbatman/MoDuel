using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Abilities;

/// <summary>
/// A link between a <see cref="IAbilityEntity"/> and an <see cref="Ability"/>> it has.
/// </summary>
public class AbilityReference {

    /// <summary>
    /// The entity that has the ability.
    /// </summary>
    public IAbilityEntity Holder;

    /// <summary>
    /// The source of the ability.
    /// </summary>
    public Source Source;

    /// <summary>
    /// The ability referenced.
    /// </summary>
    public Ability Ability;

    /// <summary>
    /// Should the ability be used.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// Information that can be stored for the ability.
    /// </summary>
    public DataTable Data = [];

    public AbilityReference(IAbilityEntity holder, Source source, Ability ability) {
        Holder = holder;
        Source = source;
        Ability = ability;
    }

    /// <summary>
    /// Gets the reaction to the provided <paramref name="trigger"/> if one exists.
    /// </summary>
    /// <returns>The reaction to the trigger or null if there is no reaction.</returns>
    public TriggerReaction? GetReaction(Trigger trigger) {
        var action = Ability.CheckTrigger(trigger);
        if (action == null)
            return null;
        return new TriggerReaction(new SourceAbility(this), action);
    }

    /// <summary>
    /// Compare this <see cref="AbilityReference"/> to another <see cref="AbilityReference"/>.
    /// </summary>
    public int CompareTo(DuelState state, AbilityReference other) {
        // Need to sum both sides to ensure parity.
        int sum = 0;
        sum += Ability.Compare(state, this, other);
        sum -= other.Ability.Compare(state, other, this);
        return sum;
    }
}
