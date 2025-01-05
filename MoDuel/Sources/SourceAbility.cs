using MoDuel.Abilities;

namespace MoDuel.Sources;

/// <summary>
/// A source derived from an ability.
/// </summary>
public class SourceAbility : Source {

    /// <summary>
    /// The specific ability reference that created the source.
    /// </summary>
    public AbilityReference Ability;

    public SourceAbility(AbilityReference ability) {
        Ability = ability;
    }

}
