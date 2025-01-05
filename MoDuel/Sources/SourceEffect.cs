using MoDuel.Effects;

namespace MoDuel.Sources;

/// <summary>
/// A source created from an <see cref="Effect"/>.
/// </summary>
public class SourceEffect : Source {

    /// <summary>
    /// The effect that created the source.
    /// </summary>
    public readonly Effect ProvidingEffect;

    public SourceEffect(Effect providingEffect) {
        ProvidingEffect = providingEffect;
    }

    /// <inheritdoc/>
    public override bool IsValid() {
        return ProvidingEffect.Source.IsValid();
    }

}
