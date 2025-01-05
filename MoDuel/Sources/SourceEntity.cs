using MoDuel.Abilities;

namespace MoDuel.Sources;

/// <summary>
/// A source from an explicit entity.
/// </summary>
public class SourceEntity : Source {

    /// <summary>
    /// The entity that created the source.
    /// </summary>
    public readonly IAbilityEntity Entity;

    public SourceEntity(IAbilityEntity entity) {
        Entity = entity;
    }
}
