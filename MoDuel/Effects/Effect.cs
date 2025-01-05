using MoDuel.Sources;

namespace MoDuel.Effects;

/// <summary>
/// An effect applied to a <see cref="Target"/>.
/// </summary>
public abstract class Effect {

    /// <summary>
    /// The provider of the effect,
    /// </summary>
    public Source Source;

    /// <summary>
    /// The target of the effect.
    /// </summary>
    public Target Target;

    public Effect(Source source, Target target) {
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Applies the effect.
    /// </summary>
    public abstract void Apply();

    /// <summary>
    /// Removes the effect. Should cancel out <see cref="Apply"/>.
    /// </summary>
    public abstract void Remove();

    /// <summary>
    /// Retrieve the name or name lookup.
    /// </summary>
    public abstract string GetName();

    /// <summary>
    /// Retrieve the description or description lookup.
    /// </summary>
    public abstract string GetDescription();

    /// <summary>
    /// Retrieve a set of parameters that can be used to build the description.
    /// </summary>
    public abstract object?[] GetParameters();


}
