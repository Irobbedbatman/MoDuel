using MoDuel.Serialization;

namespace MoDuel.Resources;

/// <summary>
/// A counter that provides a <see cref="ResourceType"/> context to a count.
/// </summary>
[SerializeReference]
public class ResourceCounter {

    /// <summary>
    /// The <see cref="ResourceType"/> this <see cref="ResourceCounter"/> uses.
    /// </summary>
    public ResourceType Resource;

    /// <summary>
    /// How much of this <see cref="Resource"/> has been counted.
    /// </summary>
    public int Count { get; set; }

    public ResourceCounter(ResourceType resourceType, int initialAmount = 0) {
        Resource = resourceType;
        Count = initialAmount;
    }

    /// <summary>
    /// The name derived from <see cref="Resource"/>.
    /// </summary>
    public string Name => Resource.Name;

    /// <summary>
    /// How much of this <see cref="Resource"/> has been counted.
    /// <para>This is an indirect way of accessing <see cref="Count"/>.</para>
    /// </summary>
    public int Amount {
        get => Count;
        set => Count = value;
    }
}
