using MoDuel;
using MoDuel.Abilities;
using MoDuel.Cards;
using MoDuel.Data.Assembled;
using MoDuel.Players;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.Triggers;

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

        card.MoveCardToGrave(card.TrueOwner);

        // TODO CLIENT: discard effects
    }

    /// <summary>
    /// The action to return all the dead cards in a <paramref name="player"/>'s grave to their respective owners hands.
    /// </summary>
    /// <param name="inflictDamage">Should the player take damage equal to the amount of cards revived.</param>
    public static void ReturnDeadCardsToHand(this Player player, bool inflictDamage = true) {

        var cards = player.Grave.ToDataSet();

        var data = new DataTable() {
            ["Player"] = player,
            ["CardsToRevive"] = cards
        };

        var state = player.Context;
        var source = new Source(); // TODO: convert to revival source.
        var trigger = new Trigger("Revive:GetCards", source, state, TriggerType.DataOverride);
        state.DataTrigger(trigger, ref data);

        if (inflictDamage)
            DamageActions.InflictDamageToPlayer(player, cards.Count);

        cards = data.Get<DataSet<CardInstance>>("CardsToRevive") ?? [];

        foreach (var card in cards) {
            if (card.InGrave) {
                var individualCardData = new DataTable() {
                    ["Card"] = card,
                    ["ReviveAction"] = new ActionFunction(CardActions.ReviveDefault)
                };

                ((IAbilityEntity)card).DataTrigger("Revive:GetAction", ref individualCardData);

                var updatedCard = individualCardData.Get<CardInstance>("Card");
                var action = individualCardData.Get<ActionFunction>("ReviveAction");

                if (updatedCard != null) {
                    action?.Call(updatedCard);
                }
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
