using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Cards;

/// <summary>
/// An instantiated version of a <see cref="Card"/>.
/// </summary>
[SerializeReference]
public partial class CardInstance : Target {

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
    /// The true owner of this card, this is typically the original owner, but ownership can still be taken.
    /// <para>Typically useful for return cards to an owners hand or grave.</para>
    /// </summary>
    public Player? TrueOwner = null;

    /// <summary>
    /// The actual original of this card. Cannot be modified.
    /// </summary>
    public readonly Player? OriginalOwner = null;

    /// <summary>
    /// The values used to define this <see cref="CardInstance"/>.
    /// </summary>
    public readonly CardMetaLoaded Meta;

    /// <summary>
    /// The context which this <see cref="CardInstance"/> was created within.
    /// </summary>
    public readonly DuelState Context;

    public CardInstance(DuelState context, CardMetaLoaded meta) : base(context.TargetRegistry) {
        Imprint = meta.Card;
        Meta = meta;
        Context = context;
        Manager = context.CardManager;
        AddImprintedAbilities();
        AddImprintedTags();
        Register();
    }

    public CardInstance(Player player, CardMetaLoaded meta) : this(player.Context, meta) {
        OriginalOwner = player;
        TrueOwner = player;
    }

    /// <summary>
    /// Constructor that creates a new card over an old one, useful for transformation.
    /// </summary>
    /// <param name="previousState">The card instance to pull original state and previous state from.</param>
    public CardInstance(CardMetaLoaded meta, CardInstance previousState) : this(previousState.Context, meta) {
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
    /// Marks this card as being truly owned bt the provided <paramref name="owner"/>.
    /// </summary>
    public CardInstance SetTrueOwner(Player? owner) {
        TrueOwner = owner;
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

}
