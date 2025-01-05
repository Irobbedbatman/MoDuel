using MoDuel.Abilities;
using MoDuel.Shared.Structures;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel.State;

/// <summary>
/// The entity that is used to provide global effects.
/// </summary>
public class GlobalEntity : Target, IAbilityEntity {

    /// <summary>
    /// The state the <see cref="GlobalEntity"/> runs under.
    /// </summary>
    public DuelState State;

    /// <summary>
    /// The list of abilities being managed.
    /// </summary>
    private readonly List<AbilityReference> Abilities = [];

    public GlobalEntity(DuelState state) : base(state.TargetRegistry) => State = state;

    /// <inheritdoc/>
    public IEnumerable<AbilityReference> GetAbilities() => Abilities;

    /// <inheritdoc/>
    public void RemoveAbility(AbilityReference ability) => Abilities.Remove(ability);

    /// <inheritdoc/>
    public void AddAbility(AbilityReference reference) => Abilities.Add(reference);

    /// <inheritdoc/>
    public DuelState GetState() => State;

    /// <summary>
    /// Calls a trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityTrigger(string triggerKey, DataTable data) => ((IAbilityEntity)this).Trigger(triggerKey, data);

    /// <summary>
    /// Calls a data trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityDataTrigger<T>(string triggerKey, ref T data) where T : DataTable => ((IAbilityEntity)this).DataTrigger(triggerKey, ref data);

}
