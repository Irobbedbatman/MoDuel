using MoDuel.Data;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Abilities;

/// <summary>
/// A set of metadata that combined together forms an ability.
/// </summary>
public abstract class Ability {

    /// <summary>
    /// Ability data that can be used when an ability is not found.
    /// </summary>
    public static readonly Ability Missing = new MissingAbilityInternal();

    /// <summary>
    /// The internal data for <see cref="Missing"/>.
    /// </summary>
    private class MissingAbilityInternal : Ability {
        public override ActionFunction? CheckTrigger(Trigger trigger) => null;

        public override string GetDescription() => "[MISSING]";

        public override string GetName() => "Missing";

        public override object[] GetParameters(AbilityReference reference) => [];
    }

    /// <summary>
    /// The <see cref="Package"/> this <see cref="Ability"/> came from.
    /// </summary>
    public Package? SourcePackage;

    /// <summary>
    /// The name or name lookup.
    /// </summary>
    public abstract string GetName();

    /// <summary>
    /// The description or description lookup.
    /// </summary>
    public abstract string GetDescription();

    /// <summary>
    /// Parameters that will be provided to the description.
    /// </summary>
    /// <param name="source">The source to get the parameters via.</param>
    public abstract object?[] GetParameters(AbilityReference reference);

    /// <summary>
    /// Retrieves the result of the provided trigger, or null if there is no valid value.
    /// </summary>
    public abstract ActionFunction? CheckTrigger(Trigger trigger);

    /// <summary>
    /// Compares the two abilities via the reference that is using them.
    /// </summary>
    public virtual int Compare(DuelState state, AbilityReference? x, AbilityReference? y) {
        return 0;
    }
}
