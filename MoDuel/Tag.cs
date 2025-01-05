using MoDuel.Sources;

namespace MoDuel;

/// <summary>
/// A string based tag that can be used to represent a fixed set of text.
/// </summary>
public class Tag {

    /// <summary>
    /// The provider of the tag.
    /// </summary>
    public Source Source;

    /// <summary>
    /// The test that represents the tag,
    /// </summary>
    public string Text;

    public Tag(string text, Source source) {
        Text = text;
        Source = source;
    }

    /// <summary>
    /// Implicit converter to the <see cref="Text"/> of the tag.
    /// </summary>
    public static implicit operator string(Tag tag) {
        return tag.Text;
    }

    /// <inheritdoc/>
    public override string ToString() => Text;

}
