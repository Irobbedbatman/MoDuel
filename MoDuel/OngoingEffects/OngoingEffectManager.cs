using MoDuel.Serialization;
using System.Collections;

namespace MoDuel.OngoingEffects;

/// <summary>
/// The manager that records and manages all the loaded <see cref="OngoingEffect"/>s that are registered.
/// </summary>
[SerializeReference]
public class OngoingEffectManager : IEnumerable<OngoingEffect> {

    /// <summary>
    /// The set of currently active ongoing effects.
    /// </summary>
    public readonly List<OngoingEffect> OngoingEffects = [];

    /// <summary>
    /// Activates the provided <paramref name="effect"/> by adding to the <see cref="OngoingEffects"/>.
    /// </summary>
    public void RegisterOngoingEffect(OngoingEffect effect) => OngoingEffects.Add(effect);

    /// <summary>
    /// Deactivates the provided <paramref name="effect"/> by removing it from the <see cref="OngoingEffects"/>.
    /// </summary>
    public bool DeregisterOngoingEffect(OngoingEffect effect) => OngoingEffects.Remove(effect);

    /// <inheritdoc/>
    public IEnumerator<OngoingEffect> GetEnumerator() => OngoingEffects.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => OngoingEffects.GetEnumerator();

    /// <summary>
    /// Get the subset of <see cref="OngoingEffect"/>s that are currently active.
    /// </summary>
    public IEnumerable<OngoingEffect> GetActiveEffects() => OngoingEffects.Where(effect => effect.Active);

}
