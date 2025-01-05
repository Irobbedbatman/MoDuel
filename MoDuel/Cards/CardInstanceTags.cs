using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.Triggers;

namespace MoDuel.Cards;

// See CardInstance.cs for documentation.
public partial class CardInstance {

    /// <summary>
    /// A collection of string tags.
    /// </summary>
    public readonly DataSet<Tag> Tags = [];

    /// <summary>
    /// Add the tags from the imprint.
    /// </summary>
    private void AddImprintedTags() {
        var tagSource = new SourceImprint(Imprint);
        foreach (var tag in Imprint.Tags) {
            Tags.Add(new Tag(tag, tagSource));
        }
    }

    /// <summary>
    /// Checks to see if the <see cref="CardInstance"/> has a tag or if it's <see cref="Imprint"/> has a tag.
    /// <para>The behaviour of this method can be changed through <see cref="Tags"/> and <see cref="TagsToHide"/>.</para>
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <param name="caseSensitive">Whether the check should be case sensitive. Case sensitive checking is faster.</param>
    /// <returns>True if the card has the tag false otherwise.</returns>
    public bool HasTag(string tag, bool caseSensitive = true) {
        return GetTags().Any(t => t.Equals(tag, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Get all the tags this card has currently. Takes into account <see cref="Imprint"/>, <see cref="Tags"/> and <see cref="TagsToHide"/>.
    /// <para>Performs an overwrite trigger under the name "CardGetTags".</para>
    /// </summary>
    /// <returns>An array containing each tag.</returns>
    public string[] GetTags() {

        DataTable overwriteTable = new() {
            { "Tags", Tags },
            { "Card", this }
        };

        Context.DataTrigger(new Trigger("CardGetTags", new Source(), Context, TriggerType.DataOverride), ref overwriteTable);
        return overwriteTable.Get<IEnumerable<Tag>>("Tags")?.Select(t => t.ToString()).ToArray() ?? [];

    }
}
