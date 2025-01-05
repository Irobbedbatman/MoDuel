using MoDuel.Abilities;
using MoDuel.Data;
using MoDuel.Shared.Structures;
using MoDuel.State;
using MoDuel.Tools;
using System.Text.Json.Nodes;

namespace MoDuel.Resources;

/// <summary>
/// A resource type loaded from a file.
/// <para>Has abilities to </para>
/// </summary>
public class ResourceType : LoadedAsset, IAbilityEntity {

#nullable disable
    public static readonly ResourceType Missing = new(null, "", null);
#nullable enable

    /// <summary>
    /// The list of abilities tied to the resource type.
    /// </summary>
    public readonly List<AbilityReference> Abilities = [];

    public ResourceType(Package package, string id, JsonObject data) : base(package, id, data) {

    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <inheritdoc/>
    public IEnumerable<AbilityReference> GetAbilities() => Abilities;

    /// <inheritdoc/>
    public void RemoveAbility(AbilityReference ability) {
        Abilities.Remove(ability);
    }

    /// <summary>
    /// Add an ability to the <see cref="ResourceType"/>.
    /// </summary>
    public void AddAbility(AbilityReference ability) {
        Abilities.Add(ability);
    }

    /// <inheritdoc/>
    public DuelState GetState() => ThreadContext.DuelState;

    /// <summary>
    /// Calls a trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityTrigger(string triggerKey, DataTable data) => ((IAbilityEntity)this).Trigger(triggerKey, data);

    /// <summary>
    /// Calls a data trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityDataTrigger<T>(string triggerKey, ref T data) where T : DataTable => ((IAbilityEntity)this).DataTrigger(triggerKey, ref data);

}
