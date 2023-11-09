using MoDuel.Serialization;
using System.Collections;

namespace MoDuel.Resources;

/// <summary>
/// A defined cost that can be compared to ensure actions can be performed.
/// </summary>
[SerializeReference]
public class ResourceCost : IEnumerable<ResourceCounter>, IComparable<ResourceCost> {

    /// <summary>
    /// Create a <see cref="ResourceCost"/> that has no elements.
    /// </summary>
    public static ResourceCost NewEmpty() => [];

    /// <summary>
    /// Each component that makes up the total cost; each part representing one <see cref="ResourceType"/>.
    /// </summary>
    public readonly Dictionary<ResourceType, ResourceCounter> Parts = [];

    public ResourceCost() { }

    public ResourceCost(ICollection<ResourceCounter> parts) {
        foreach (var part in parts) {
            Parts.Add(part.Resource, part);
        }
    }

    /// <summary>
    /// Indexer for each component of the <see cref="Parts"/> collection.
    /// </summary>
    /// <returns>The <see cref="ResourceCounter"/> from <see cref="Parts"/> if one was found; <c>null</c> otherwise.</returns>
    public ResourceCounter? this[ResourceType resource] {
        get {
            if (Parts.TryGetValue(resource, out ResourceCounter? resourceCounter))
                return resourceCounter;
            return null;
        }
    }

    /// <summary>
    /// Indexer that redirects to <see cref="this[ResourceType]"/> after getting the <see cref="ResourceType"/> through <see cref="GetCounterByName(string)"/>.
    /// </summary>
    public ResourceCounter? this[string resourceName] => GetCounterByName(resourceName);

    /// <summary>
    /// Increases the cost of the specific <paramref name="resource"/> by the <paramref name="amount"/> provided.
    /// <para>This will change the <see cref="Parts"/> item of the <paramref name="resource"/>; creating it if had not been created yet.</para>
    /// </summary>
    public void Add(ResourceType resource, int amount) {
        var counter = this[resource];
        if (counter == null) {
            // Create the counter if it does not exist.
            counter = new ResourceCounter(resource);
            Parts.Add(resource, counter);
        }
        counter.Count += amount;
    }

    /// <summary>
    /// Increases the cost by the provided counter.
    /// <para>This will change the <see cref="Parts"/> item of the <see cref="ResourceCounter.Resource"/>; creating it if had not been created yet.</para>
    /// </summary>
    public void Add(ResourceCounter counter) => Add(counter.Resource, counter.Amount);

    /// <summary>
    /// Reduces the cost of the specific <paramref name="resource"/> by the <paramref name="amount"/> provided.
    /// <para>Will do nothing if <see cref="Parts"/> does not contain the <paramref name="resource"/>.</para>
    /// </summary>
    /// <param name="allowBelow0">Can the counter be reduced below 0. If false; when reducing by amount the value will be clamped to 0 or above after the deduction.</param>
    public void Deduct(ResourceType resource, int amount, bool allowBelow0 = false) {
        var counter = this[resource];
        if (counter == null)
            return;
        counter.Count -= amount;
        // Clamp to 0 or above when allowBelow0 has not been set to true.
        if (!allowBelow0 && counter.Count < 0)
            counter.Count = 0;
    }

    /// <summary>
    /// Reduces the cost by the provide resource <paramref name="counter"/>.
    /// <para>Will do nothing if <see cref="Parts"/> does not contain the <see cref="ResourceCounter.Resource"/>.</para>
    /// </summary>
    /// <param name="allowBelow0">Can the counter be reduced below 0. If false; when reducing by amount the value will be clamped to 0 or above after the deduction.</param>
    public void Deduct(ResourceCounter counter, bool allowBelow0 = false) => Deduct(counter.Resource, counter.Count, allowBelow0);

    /// <summary>
    /// Gets a <see cref="ResourceCounter"/> from parts by checking the provided <paramref name="resourceName"/> against the name of each <see cref="ResourceType"/>.
    /// </summary>
    public ResourceCounter? GetCounterByName(string resourceName) {
        return Parts.Values.FirstOrDefault((resourceCounter) => {
            return resourceCounter.Name == resourceName;
        });
    }

    public IEnumerator<ResourceCounter> GetEnumerator() => Parts.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Parts.Values.GetEnumerator();

    public override string ToString() {
        if (Parts.Count > 0)
            return "{" + string.Join(", ", Parts.Select((part) => { return part.Key.Name + ": " + part.Value.Count.ToString(); })) + "}";
        return "{}";
    }

    /// <summary>
    /// Compares against another <see cref="ResourceCost"/> comparing each part found in <see cref="Parts"/>.
    /// <para>Performs an explicit comparison and if that comparison provides no result compares raw values.</para>
    /// </summary>
    /// <returns>A positive number if <c>this</c> costs more than the <paramref name="other"/> <see cref="ResourceCost"/>.</returns>
    public int CompareTo(ResourceCost? other) {

        int compareSum = 0;

        if (other == null)
            return 0;

        foreach (var part in Parts.Keys.Union(other.Parts.Keys)) {

            // Compare using explicit trigger on the resource type.
            int compare = part.Trigger("Compare", [
                Parts[part],
                other[part]
            ]) ?? 0;


            // If the result of compare comes back indecisive compare the counts.
            // Both resource costs may not have the part so null validation is required.
            if (compare == 0)
                compare = (Parts[part]?.Count ?? 0) - (other[part]?.Count ?? 0);

            // Add to the sum.
            compareSum += compare;

        }

        return compareSum;
    }

    /// <summary>
    /// Combiner for two <see cref="ResourceCost"/>s.
    /// <para>Will return a <see cref="ResourceCost"/> with each unique <see cref="ResourceType"/> found in <paramref name="a"/> and <paramref name="b"/>.</para>
    /// </summary>
    public static ResourceCost operator +(ResourceCost a, ResourceCost b) {

        ResourceCost combined = [];

        // Get the parts from both costs. This ensures that the output has all required parts.
        foreach (var resource in a.Parts.Keys.Union(b.Parts.Keys)) {

            // Create the counter for the resource.
            ResourceCounter counter = new(resource) {
                // Add to the counter for both costs if they have the part.
                Count = (a?[resource]?.Count ?? 0)
                    + (b?[resource]?.Count ?? 0)
            };

            combined.Parts.Add(resource, counter);
        }

        return combined;
    }

}
