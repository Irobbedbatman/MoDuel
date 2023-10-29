using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Players;

namespace DefaultPackage;
public static class PlayerActions {


    public static void ChangeHero() {
        // TODO DELAY: Change Hero.
    }

    /// <summary>
    /// The action to discard the <paramref name="card"/> from the players hand.
    /// </summary>
    [ActionName("DiscardCard")]
    public static void Discard(this Player player, CardInstance card) {
        if (card.Owner != player || !card.InHand) {
            GlobalActions.Fizzle();
            return;
        }

        card.MoveCardToGrave(card.OriginalOwner);

        // TODO CLIENT: discard effects
    }

    /// <summary>
    /// The action to return all the dead cards in a <paramref name="player"/>'s grave to their respective owners hands.
    /// </summary>
    /// <param name="inflictDamage">Should the player take damage equal to the amount of cards revived.</param>
    public static void ReturnDeadCardsToHand(this Player player, bool inflictDamage = true) {

        var cards = player.Grave.ToHashSet();

        // TODO DELAY: Get cards overwirite i guess.

        foreach (var card in player.Grave) {
            bool result = card.FallbackTrigger("CanBeRevived", new MoDuel.ActionFunction(CardActions.CanBeRevivedDefault));

            if (!result) {
                cards.Remove(card);
            }
        }

        if (inflictDamage)
            DamageActions.InflictDamageToPlayer(player, cards.Count);


        foreach (var card in cards) {
            if (card.InGrave) {
                card.FallbackTrigger("Revive", new MoDuel.ActionFunction(CardActions.ReviveDefault));
            }
        }
    }

    [ActionName("KillPlayer")]
    public static void Kill(this Player player) {
        var state = player.Context;
        state.Finished = true;
        var winner = state.GetOpposingPlayer(player);
        Console.WriteLine($"The winner is is {winner.UserId}");
        // TODO CLIENT: send end of game confirmation.
    }

}
