using MoDuel.Serialization;
using System.Collections;

namespace MoDuel.Resources;

/// <summary>
/// A set of <see cref="ResourceCounter"/>s that can be referenced by each counter's <see cref="ResourceType"/> or their names.
/// </summary>
[SerializeReference]
public class ResourcePool : IEnumerable<ResourceCounter> {

    /// <summary>
    /// The hidden dictionary that ties <see cref="ResourceType"/> to a <see cref="ResourceCounter"/>.
    /// </summary>
    private readonly Dictionary<ResourceType, ResourceCounter> _pool = new();

    /// <summary>
    /// Constructor that converts a collection of <see cref="ResourceType"/>s and turns them into a <see cref="ResourcePool"/>
    /// </summary>
    /// <param name="resources">A collection of required <see cref="ResourceType"/>s for this <see cref="ResourcePool"/></param>
    public ResourcePool(IEnumerable<ResourceType> resources) {
        foreach (var resource in resources)
            _pool.Add(resource, new(resource));
    }

    /// <summary>
    /// Constructor that creates the mana pool from the provided <paramref name="resouceCounters"/>.
    /// </summary>
    /// <param name="resouceCounters">The counter for each resource.</param>
    public ResourcePool(IEnumerable<ResourceCounter> resouceCounters) {
        foreach (var counter in resouceCounters) {
            _pool.Add(counter.Resource, counter);
        }
    }

    /// <summary>
    /// Implicit operator that converts a <see cref="ResourcePool"/> into the an array of <see cref="ResourceCounter"/>
    /// <para>Only <see cref="IEnumerable{T}"/> public accessor of <see cref="ResourcePool"/>.</para>
    /// </summary>
    /// <param name="resource">The <see cref="ResourcePool"/> to convet.</param>
    public static implicit operator ResourceCounter[](ResourcePool resource) { return resource._pool.Values.ToArray(); }

    /// <summary>
    /// Retrieve from <see cref="ResourcePool"/> using a <see cref="ResourceType"/> parsed from a string.
    /// </summary>
    /// <param name="manatypestring">The string to use <see cref="Enum.Parse(Type, string)"/> on.</param>
    /// <returns>A <see cref="ResourceCounter"/> tied to a <see cref="CostType"/></returns>
    public ResourceCounter? this[string manatypestring] => GetCounterByName(manatypestring) ?? null;

    /// <summary>
    /// Retrieve from <see cref="ResourcePool"/> using a <see cref="ResourceType"/>.
    /// </summary>
    /// <param name="type">The type of <see cref="ResourceCounter"/> we want to access.</param>
    public ResourceCounter? this[ResourceType type] {
        get {
            if (_pool.TryGetValue(type, out ResourceCounter? counter))
                return counter;
            return null;
        }
    }

    /// <summary>
    /// How many <see cref="ResourceCounter"/>s are in the <see cref="ResourcePool"/>.
    /// </summary>
    public int Length => _pool.Count;

    /// <summary>
    /// Check to see if the <see cref="ResourcePool"/> has the provided <see cref="ResourceType"/>.
    /// </summary>
    public bool HasType(ResourceType type) => _pool.ContainsKey(type);

    /// <summary>
    /// Check to see the <see cref="ResourcePool"/> has a <see cref="ResourceType"/> with the name <paramref name="resourceName"/>.
    /// </summary>
    public bool HasType(string resourceName) => GetCounterByName(resourceName) != null;

    /// <summary>
    /// Gets the <see cref="ResourceCounter"/> from the <see cref="ResourcePool"/> that has a matching <paramref name="resourceName"/>.
    /// </summary>
    /// <param name="resourceName">The name of the <see cref="ResourceType"/> that the <see cref="ResourceCounter"/> uses.</param>
    /// <returns>The <see cref="ResourceCounter"/> requested or <c>null</c> if it dould not be found.</returns>
    public ResourceCounter? GetCounterByName(string resourceName) {
        return _pool.Values.FirstOrDefault((resourceCounter) => {
            return resourceCounter.Name == resourceName;
        });
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<ResourceCounter> GetEnumerator() => _pool.Values.GetEnumerator();
}
