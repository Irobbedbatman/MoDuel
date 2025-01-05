using MoDuel.Data;
using MoDuel.Shared.Json;
using System.Collections;
using System.Text.Json.Nodes;

namespace MoDuel.Resources;

/// <summary>
/// The set of <see cref="CostComponent"/>s that create a full cost.
/// </summary>
public class ResourceCost : IEnumerable<CostComponent>, ICloneable {

    /// <summary>
    /// The individual components of the full cost.
    /// </summary>
    public readonly List<CostComponent> CostNodes = [];

    public ResourceCost() { }

    public ResourceCost(IEnumerable<CostComponent> components) {
        CostNodes.AddRange(components);
    }

    public ResourceCost(Package sender, JsonNode? data) {
        if (data != null) {
            foreach (var costNode in data.GetValues()) {
                CostNodes.Add(new CostComponent(sender, costNode));
            }
        }
    }

    /// <summary>
    /// Convert the cost to a set of generic counters, this does not reflect how it may actual be applied.
    /// <para>Flattens the counters into each type.</para>
    /// </summary>
    public ResourceCounter[] GetAsCounters() => CostNodes.GroupBy(cn => cn.Type).Select(g => new ResourceCounter(g.Key, g.Sum(c => c.Amount))).ToArray();


    /// <summary>
    /// Get all the components of the provided <paramref name="type"/>.
    /// </summary>
    public IEnumerable<CostComponent> GetComponentsOfType(ResourceType type) => CostNodes.Where(c => c.Type == type);

    /// <summary>
    /// Add the provided cost component on top of the present cost.
    /// </summary>
    public void AddC(CostComponent component) => CostNodes.Add(component);

    /// <summary>
    /// Add the provided cost on top of the present cost.
    /// </summary>
    public void Add(IEnumerable<CostComponent> costNodes) => CostNodes.AddRange(costNodes);

    public IEnumerator<CostComponent> GetEnumerator() => ((IEnumerable<CostComponent>)CostNodes).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)CostNodes).GetEnumerator();

    /// <summary>
    /// Create a new cost with same components.
    /// </summary>
    public ResourceCost Clone() => new(CostNodes);

    /// <inheritdoc/>
    object ICloneable.Clone() => Clone();
}
