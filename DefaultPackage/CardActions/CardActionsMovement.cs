using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Players;

namespace DefaultPackage;

// Actions to move a card to another location.
public static partial class CardActions {

    /// <summary>
    /// Removes the provided <paramref name="card"/> from either the grave or the hand if it is one of those locations.
    /// </summary>
    [ActionName(nameof(RemoveFromCurrentLocation))]
    public static void RemoveFromCurrentLocation(this CardInstance card) {
        if (card.InGrave) {
            RemoveFromGrave(card);
        }
        else if (card.InHand) {
            RemoveFromHand(card);
        }
    }

    /// <summary>
    /// Moves a <paramref name="card"/> to a <paramref name="player"/>s hand.
    /// <para>If <paramref name="player"/> is null the card is attempted to move to it's true owners hand.</para>
    /// <para>If the original owner does not exist the card will only be removed from its current location.</para>
    /// </summary>
    public static void MoveCardToHand(this CardInstance card, Player? player = null) {
        RemoveFromCurrentLocation(card);
        player ??= card.TrueOwner;
        if (player == null) return;
        player.AddCardToHand(card);
        // TOOD CLIENT: add card to hand client.
    }

    /// <summary>
    /// Moves a <paramref name="card"/> to a <paramref name="player"/>s grave.
    /// <para>If <paramref name="player"/> is null the card is attempted to move to it's true owners greave.</para>
    /// <para>If the original owner does not exist the card will only be removed from its current location.</para>
    /// </summary>
    public static void MoveCardToGrave(this CardInstance card, Player? player = null) {
        RemoveFromCurrentLocation(card);
        if (player == null)
            player = card.TrueOwner;
        if (player == null) return;
        player.AddCardToGrave(card);
        // TODO CLIENT: add card to grave client.
    }

    /// <summary>
    /// Removes the <paramref name="card"/> from the hand that it is in if it is in a hand.
    /// </summary>
    /// <param name="card"></param>
    public static void RemoveFromHand(this CardInstance card) {
        if (card.Owner == null) return;
        card.Owner.RemoveCardFromHand(card);
        // TODO CLIENT: Remove card from hand client.
    }

    /// <summary>
    /// Removes the <paramref name="card"/> from the grave that it is in if it is in a grave.
    /// </summary>
    /// <param name="card"></param>
    public static void RemoveFromGrave(this CardInstance card) {
        if (card.Owner == null) return;
        card.Owner.RemoveCardFromGrave(card);
        // TODO CLIENT: Remove card from grave client.
    }


}
