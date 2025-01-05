using MoDuel.Abilities;
using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;

namespace MoDuel.Heroes;

/// <summary>
/// An instanced version of <see cref="Hero"/> stored in a <see cref="Player"/>.
/// </summary>
[SerializeReference]
public class HeroInstance : IAbilityEntity {

    /// <summary>
    /// The <see cref="Hero"/> this <see cref="HeroInstance"/> is taken from.
    /// </summary>
    public readonly Hero Imprint;

    /// <summary>
    /// The <see cref="Player"/> that is using this <see cref="HeroInstance"/> as their <see cref="Player.Hero"/>.
    /// </summary>
    public readonly Player Owner;

    /// <summary>
    /// Abilities on this instance of the hero.
    /// </summary>
    public List<AbilityReference> Abilities = [];

    public HeroInstance(Hero hero, Player owner) {
        Imprint = hero;
        Owner = owner;
        AddImprintedAbilities();
    }

    /// <summary>
    /// Add the abilities supplied byt the <see cref="Hero"/> <see cref="Imprint"/>.
    /// </summary>
    private void AddImprintedAbilities() {
        foreach (var ability in Imprint.InitialAbilities) {
            Abilities.Add(new AbilityReference(this, new SourceImprint(Imprint), ability));
        }
    }

    /// <inheritdoc/>
    public IEnumerable<AbilityReference> GetAbilities() => Abilities;

    /// <inheritdoc/>
    public void RemoveAbility(AbilityReference ability) => Abilities.Remove(ability);

    /// <inheritdoc/>
    public DuelState GetState() => Owner.Context;

    /// <inheritdoc/>
    public void AddAbility(AbilityReference ability) => Abilities.Add(ability);

    /// <summary>
    /// Calls a trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityTrigger(string triggerKey, DataTable data) => ((IAbilityEntity)this).Trigger(triggerKey, data);

    /// <summary>
    /// Calls a data trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityDataTrigger<T>(string triggerKey, ref T data) where T : DataTable => ((IAbilityEntity)this).DataTrigger(triggerKey, ref data);

}
