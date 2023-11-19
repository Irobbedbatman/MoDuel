using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Cards;

/// <summary>
/// An instantiated version of a <see cref="Card"/>.
/// </summary>
[SerializeReference]
public partial class CardInstance : Target, IImplicitTriggerable, IExplicitTriggerable {

    /// <summary>
    /// The loaded <see cref="Card"/> this <see cref="CardInstance"/> derives from.
    /// </summary>
    public readonly Card Imprint;

    /// <summary>
    /// The container that will be used to get if this <see cref="CardInstance"/> is currently listing to implicit triggers.
    /// </summary>
    public readonly CardInstanceManager Manager;

    /// <summary>
    /// The current owner of this card.
    /// </summary>
    public Player? Owner = null;

    /// <summary>
    /// The owner of this card originally.
    /// </summary>
    public Player? OriginalOwner = null;

    /// <summary>
    /// Trigger reactions that are used before ones that are found in <see cref="Imprint"/>.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> NewTriggerReactions = [];
    /// <summary>
    /// Explicit trigger reactions that are used before ones that are found in <see cref="Imprint"/>.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> NewExplicitTriggerReactions = [];

    /// <summary>
    /// The values used to define this <see cref="CardInstance"/>.
    /// </summary>
    public readonly CardMeta Meta;

    /// <summary>
    /// The context which this <see cref="CardInstance"/> was created within.
    /// </summary>
    public readonly DuelState Context;

    /// <summary>
    /// A collection of string tags that is checked before <see cref="Imprint"/> has tag..
    /// </summary>
    public readonly HashSet<string> Tags = [];

    /// <summary>
    /// A collection of string tags return false before <see cref="Imprint"/> has tag.
    /// <para>Also hides them from <see cref="Tags"/>.</para>
    /// </summary>
    public readonly HashSet<string> TagsToHide = [];

    /// <summary>
    /// The card this level should be treated as instead of it;s <see cref="Owner"/>s.
    /// <para>Should not affect the level of <see cref="CardInstance"/s.></para>
    /// </summary>
    public int? FixedLevel;

    public CardInstance(DuelState context, CardMeta meta) : base(context.TargetRegistry) {
        Imprint = meta.Card;
        Meta = meta;
        Context = context;
        Manager = context.CardManager;
        Register();
    }

    public CardInstance(Player player, CardMeta meta) : this(player.Context, meta) {
        OriginalOwner = player;
    }

    /// <summary>
    /// Constructor that creates a new card over an old one, useful for transformation.
    /// </summary>
    /// <param name="previousState">The card instance to pull original state and previous state from.</param>
    public CardInstance(CardMeta meta, CardInstance previousState) : this(previousState.Context, meta) {
        PreviousState = previousState;
    }

    /// <summary>
    /// Marks this card as being owned by the provided <paramref name="owner"/> .
    /// </summary>
    public CardInstance SetOwner(Player? owner) {
        Owner = owner;
        return this;
    }

    /// <summary>
    /// Checks to see if this card is imprinted off of a card with a given id.
    /// </summary>
    /// <param name="cardId"><see cref="Card.Id"/></param>
    /// <returns>True if the card imprints id is equivalent to the given id.</returns>
    public bool MatchesId(string cardId) => Imprint.Id == cardId;

    /// <summary>
    /// Activates the card instance allowing it to react to triggers.
    /// </summary>
    public void Register() => Manager.Register(this);
    /// <summary>
    /// Deactivate the card instance stopping it from reacting to triggers.
    /// </summary>
    public void Deregister() => Manager.Deregister(this);

    /// <summary>
    /// Add a trigger reaction to this <see cref="CardInstance"/>.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being added an explicit trigger or an implicit trigger.</param>
    public void AddTrigger(string triggerKey, ActionFunction triggerReaction, bool isExplicit = false) {
        if (!isExplicit)
            NewTriggerReactions.Add(triggerKey, triggerReaction);
        else
            NewExplicitTriggerReactions.Add(triggerKey, triggerReaction);
    }

    /// <summary>
    /// Adds an explicit reaction to this <see cref="CardInstance"/>.
    /// </summary>
    public void AddExplicitTrigger(string triggerKey, ActionFunction reaction) => AddTrigger(triggerKey, reaction, true);

    /// <summary>
    /// Removes a trigger reaction.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being removed an explicit trigger.</param>
    public void RemoveTrigger(string triggerKey, bool isExplicit = false) {
        if (!isExplicit)
            NewTriggerReactions.Remove(triggerKey);
        else
            NewExplicitTriggerReactions.Remove(triggerKey);
    }
    /// <summary>
    /// Removes an explicit trigger reaction.
    /// </summary>
    public void RemoveExplicitTrigger(string triggerKey) => NewExplicitTriggerReactions.Remove(triggerKey);

    /// <inheritdoc/>
    public ActionFunction GetExplicitReaction(string trigger) {
        // Prioritise reactions on this instance first.
        if (NewExplicitTriggerReactions.TryGetValue(trigger, out var value))
            return value;
        if (Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out value))
            return value;
        return new ActionFunction();
    }

    /// <inheritdoc/>
    public ActionFunction GetImplicitReaction(string trigger) {
        // Prioritise reactions on this instance first.
        if (NewTriggerReactions.TryGetValue(trigger, out var value))
            return value;
        if (Imprint.TriggerReactions.TryGetValue(trigger, out value))
            return value;
        return new ActionFunction();
    }

    /// <inheritdoc/>
    public bool HasImplicitReaction(string trigger) => NewTriggerReactions.ContainsKey(trigger) || Imprint.TriggerReactions.ContainsKey(trigger);

    /// <inheritdoc/>
    public dynamic? Trigger(string trigger, params object?[] arguments) {
        return GetExplicitReaction(trigger)?.Call(arguments.Prepend(this).ToArray());
    }

    /// <inheritdoc/>
    public dynamic? FallbackTrigger(string trigger, ActionFunction fallback, params object?[] arguments) {
        var reaction = GetExplicitReaction(trigger);

        if (reaction?.IsAssigned ?? false)
            return reaction?.Call(arguments.Prepend(this).ToArray());
        return fallback?.Call(arguments.Prepend(this).ToArray());
    }

    /// <inheritdoc/>
    public dynamic? FallbackTrigger(string trigger, Delegate fallback, params object?[] arguments) {
        return FallbackTrigger(trigger, new ActionFunction(fallback), arguments);
    }

    /// <summary>
    /// Checks to see if the <see cref="CardInstance"/> has a tag or if it's <see cref="Imprint"/> has a tag.
    /// <para>The behaviour of this method can be changed through <see cref="Tags"/> and <see cref="TagsToHide"/>.</para>
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <param name="caseSensitive">Whether the check should be case sensitive. Case sensitive checking is faster.</param>
    /// <returns>True if the card has the tag false otherwise.</returns>
    public bool HasTag(string tag, bool caseSensitive = true) {
        bool result = GetTags().Any(t => t.Equals(tag, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase));
        return result;
    }

    /// <summary>
    /// Get all the tags this card has currently. Takes into account <see cref="Imprint"/>, <see cref="Tags"/> and <see cref="TagsToHide"/>.
    /// <para>Performs an overwrite trigger under the name "CardGetTags".</para>
    /// </summary>
    /// <returns>An array containing each tag.</returns>
    public string[] GetTags() {
        var tags = Imprint.Tags.Union(Tags).Except(TagsToHide).ToList();
        OverwriteTable overwriteTable = new() {
            { "Tags", tags },
            { "Card", this }
        };
        Context.OverwriteTrigger("CardGetTags", overwriteTable);
        return overwriteTable.Get<IEnumerable<string>>("Tags")?.ToArray() ?? [];

    }

}
