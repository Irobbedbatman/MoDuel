namespace MoDuel.Sources;

/// <summary>
/// An object to provide reason as to why another thing exists.
/// </summary>
public class Source {

    /// <summary>
    /// An internal value for <see cref="IsValid"/> for sources that are typically constant or could be implied through a state change.
    /// </summary>
    protected bool isValid = true;

    public Source() { }

    /// <summary>
    /// Checks to see if the source is still valid.
    /// </summary>
    public virtual bool IsValid() => isValid;

}
