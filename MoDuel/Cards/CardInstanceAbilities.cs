using MoDuel.Abilities;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Cards;

// See CardInstance.cs for documentation.
public partial class CardInstance : IAbilityEntity {

    /// <summary>
    /// The list of abilities on this instance of the card.
    /// </summary>
    public List<AbilityReference> Abilities = [];

    /// <summary>
    /// Add the abilities supplied byt the <see cref="Card"/> <see cref="Imprint"/>.
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
    public DuelState GetState() => Context;

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
