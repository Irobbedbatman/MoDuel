namespace MoDuel.Sources;

/// <summary>
/// A source applied to an object that derives from an imprint such as <see cref="Cards.Card"/>.
/// </summary>
public class SourceImprint : Source {

    /// <summary>
    /// The imprint that created the source.
    /// </summary>
    public readonly object Imprint;

    public SourceImprint(object imprint) {
        Imprint = imprint;
    }

}
