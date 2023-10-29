using MoDuel.Client;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.Serialization;
using MoDuel.State;
using MoDuel.Triggers;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel.Cards;

/// <summary>
/// An instantiated version of a <see cref="Card"/>.
/// </summary>
[SerializeReference]
public class CardInstance : Target, IImplicitTriggerable, IExplicitTriggerable {

    /// <summary>
    /// The loaded <see cref="Card"/> this <see cref="CardInstance"/> derives from.
    /// </summary>
    public readonly Card Imprint;

    /// <summary>
    /// The container that will be used to get if this <see cref="CardInstance"/> is currently listing to implicit triggers.
    /// </summary>
    public readonly CardInstanceManager Manager;

    /// <summary>
    /// The form this card was in previously.
    /// <para><see cref="null"/> if this is the original form.</para>
    /// </summary>
    public readonly CardInstance? PreviousState = null;

    /// <summary>
    /// The form this card was when all of its <see cref="PreviousState"/>s are gone through.
    /// </summary>
    public CardInstance OriginalState => PreviousState ?? this;

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
    public readonly Dictionary<string, ActionFunction> NewTriggerReactions = new();
    /// <summary>
    /// Explicit trigger reactions that are used before ones that are found in <see cref="Imprint"/>.
    /// </summary>
    public readonly Dictionary<string, ActionFunction> NewExplicitTriggerReactions = new();

    /// <summary>
    /// The values used to define this card instace.
    /// <para>Not all cards are neccassirly created with <see cref="CardMeta"/>.</para>
    /// </summary>
    public readonly CardMeta Meta;

    /// <summary>
    /// The context which this card instace was created within.
    /// </summary>
    public readonly DuelState Context;

    /// <summary>
    /// A collection of string tags that is checked before <see cref="Imprint"/> has tag..
    /// </summary>
    public readonly HashSet<string> Tags = new();

    /// <summary>
    /// A collection of string tags reutrn false before <see cref="Imprint"/> has tag.
    /// <para>Also hides them from <see cref="Tags"/>.</para>
    /// </summary>
    public readonly HashSet<string> TagsToHide = new();

    /// <summary>
    /// The card this level should be treated as indstead of it;s <see cref="Owner"/>s.
    /// <para>Should not affect the level of <see cref="CreatureInstance"/s.></para>
    /// </summary>
    public int? FixedLevel;

    /// <summary>
    /// Shared active state of <see cref="ExplicitTriggerActive"/> and <see cref="ImplicitTriggerActive"/>.
    /// <para>Used to simplfy disabling both.</para>
    /// </summary>
    public bool Active {
        get => ExplicitTriggerActive && ImplicitTriggerActive;
        set {
            ExplicitTriggerActive = value;
            ImplicitTriggerActive = false;
        }
    }

    /// <summary>
    /// If the explicit triggering should be used externally.
    /// </summary>
    public bool ExplicitTriggerActive = true;

    /// <summary>
    /// If the implicit triggering should be used externally
    /// </summary>
    public bool ImplicitTriggerActive = true;

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
    /// Marks this card as being owbed by the provided <paramref name="owner"/> .
    /// </summary>
    public CardInstance SetOwner(Player? owner) {
        Owner = owner;
        return this;
    }

    /// <summary>
    /// Get the level that this card should display. Creatures have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedLevel() {
        return FallbackTrigger(nameof(GetDisplayedLevel), Context.Settings.CardGetDisplayedLevelFallback);
    }

    /// <summary>
    /// Get the cost that this card should display. Creatures have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public ResourceCost? GetDisplayedCost() {
        return FallbackTrigger(nameof(GetDisplayedCost), Context.Settings.CardGetDisplayedCostFallback);
    }

    /// <summary>
    /// Get the attack that this card should display. Creatures have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedAttack() {
        return FallbackTrigger(nameof(GetDisplayedAttack), Context.Settings.CardGetDisplayedAttackFallback);
    }


    /// <summary>
    /// Get the armour that this card should display. Creatures have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedArmour() {
        return FallbackTrigger(nameof(GetDisplayedArmour), Context.Settings.CardGetDisplayedArmourFallback);
    }

    /// <summary>
    /// Get the life that this card should display. Creatures have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedLife() {
        return FallbackTrigger(nameof(GetDisplayedLife), Context.Settings.CardGetDisplayedLifeFallback);
    }

    /// <summary>
    /// Get the description that this card should display.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// TOOD: Determine what a description should be.
    /// </summary>
    public ParametizedBlock[] GetDisplayedDescription() {
        return FallbackTrigger(nameof(GetDisplayedDescription), Context.Settings.CardGetisplayeDescriptionFallback) ?? Array.Empty<ParametizedBlock>();
    }

    /// <summary>
    /// Is this card in it's owners Hand.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Owner))]
    public bool InHand => Owner?.IsCardInHand(this) ?? false;

    /// <summary>
    /// Is this card in it's owners Grave.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Owner))]
    public bool InGrave => Owner?.IsCardInGrave(this) ?? false;

    /// <summary>
    /// Checks to see if this card is imprinted off of a card with a given id.
    /// </summary>
    /// <param name="cardId"><see cref="Card.Id"/></param>
    /// <returns>True if the card imprints id is equivalent to the given id.</returns>
    public bool MatchesId(string cardId) => Imprint.Id == cardId;

    /// <summary>
    /// Check to see if this card instance is alive. Always false for card instances but for creatures it can vary.
    /// </summary>
    public virtual bool IsAlive => false;

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
    /// <param name="isExplicit">Is the trigger that is being added an explicit trigger or an implicit tirgger.</param>
    public void AddTrigger(string triggerKey, ActionFunction triggerReaction, bool isExplicit = false) {
        if (!isExplicit)
            NewTriggerReactions.Add(triggerKey, triggerReaction);
        else
            NewExplicitTriggerReactions.Add(triggerKey, triggerReaction);
    }

    /// <summary>
    /// Adds an explicit reacion to this <see cref="CardInstance"/>.
    /// </summary>
    public void AddExplicitTrigger(string triggerKey, ActionFunction reaction) => AddTrigger(triggerKey, reaction, true);

    /// <summary>
    /// Removes a trigger reaction.
    /// </summary>
    /// <param name="isExplicit">Is the trigger that is being remvoed an explicit trigger.</param>
    public void RemoveTrigger(string triggerKey, bool isExplicit = false) {
        if (!isExplicit)
            NewTriggerReactions.Remove(triggerKey);
        else
            NewExplicitTriggerReactions.Remove(triggerKey);
    }
    /// <summary>
    /// Removes an explicit trigger reaction.
    /// </summary>
    public void RemoveExplictTrigger(string triggerKey) => NewExplicitTriggerReactions.Remove(triggerKey);

    /// <inheritdoc/>
    public ActionFunction GetExplicitReaction(string trigger) {
        // Priortise reactions on this instance first.
        if (NewExplicitTriggerReactions.TryGetValue(trigger, out var value))
            return value;
        if (Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out value))
            return value;
        return new ActionFunction();
    }

    /// <inheritdoc/>
    public ActionFunction GetImplicitReaction(string trigger) {
        // Priortise reactions on this instance first.
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
    /// <param name="caseSensitive">Wether the check should be case sensitive. Case sensitive checking is faster.</param>
    /// <returns></returns>
    public bool HasTag(string tag, bool caseSensitive = true) {
        if (caseSensitive) {
            if (TagsToHide.Contains(tag)) return false;
            if (Tags.Contains(tag)) return true;
            return Imprint.HasTag(tag, true);
        }
        else {
            tag = tag.ToLowerInvariant();
            if (TagsToHide.Select((t) => t.ToLowerInvariant()).Contains(tag)) return false;
            if (Tags.Select((t) => t.ToLowerInvariant()).Contains(tag)) return true;
            return Imprint.HasTag(tag, false);
        }
    }

}
