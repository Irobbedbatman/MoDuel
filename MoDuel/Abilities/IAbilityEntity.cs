using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Abilities;

/// <summary>
/// An entity that can have a set of <see cref="Ability"/>.
/// </summary>
public interface IAbilityEntity {

    /// <summary>
    /// All the abilities on the <see cref="IAbilityEntity"/>.
    /// </summary>
    public IEnumerable<AbilityReference> GetAbilities();

    /// <summary>
    /// Get the state the entity is instanced in.
    /// </summary>
    public DuelState GetState();

    /// <summary>
    /// Adds the provided <paramref name="ability"/> to the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AddAbility(AbilityReference ability);

    /// <summary>
    /// Remove the provided <paramref name="ability"/> from the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void RemoveAbility(AbilityReference ability);

    /// <summary>
    /// Explicitly call a trigger on the entity to call it's reactions in sequence.
    /// </summary>
    public void Trigger(string triggerKey, DataTable data) {
        var state = GetState();
        var trigger = new Trigger(triggerKey, new SourceEntity(this), state, TriggerType.Explicit);
        state.ExplicitTrigger(trigger, data);
    } 

    /// <summary>
    /// Explicitly call a trigger on the entity to update the provided <paramref name="data"/>.
    /// </summary>
    public void DataTrigger<T>(string triggerKey, ref T data) where T : DataTable{
        var state = GetState();
        var trigger = new Trigger(triggerKey, new SourceEntity(this), state, TriggerType.ExplicitData);
        state.ExplicitDataTrigger(trigger, ref data);
    }

    /// <summary>
    /// Get all the reactions this entity has to the provided <paramref name="trigger"/>.
    /// </summary>
    public IEnumerable<TriggerReaction> GetReactions(Trigger trigger) {
        return GetAbilities().Select(a => a.GetReaction(trigger)).OfType<TriggerReaction>().Order();
    }

}

