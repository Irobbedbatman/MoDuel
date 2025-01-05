using MoDuel;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Field;
using MoDuel.Resources;
using MoDuel.Shared.Structures;

namespace DefaultPackage;

/// <summary>
/// The default actions that occurs when a card is played based on its type.
/// </summary>
public static class DefaultPlayActions {

    /// <summary>
    /// The generic play card action that will branch to there other play card actions based on the card's type.s
    /// </summary>
    [ActionName(nameof(PlayCard))]
    public static void PlayCard(CardInstance card, Target target) {
        if (card.Imprint.Type == "Creature") {
            PlayCardCreature(card, target);
        }
    }

    /// <summary>
    /// The default action when a creature type card is played.
    /// </summary>
    [ActionName(nameof(PlayCardCreature))]
    public static void PlayCardCreature(CardInstance card, Target target) {

        if (target is not FieldSlot slot || slot.IsOccupied || slot.ParentField.Owner != card.Owner)
            return;

        var data = new DataTable() {
            ["Level"] = CardActions.GetLevelDefault(card),
        };

        card.AbilityDataTrigger("CardPlayed:Level", ref data);

        int level = data.Get<int>("Level");
        data["Cost"] = CardActions.GetCostDefault(card, CardActions.GetLevelDefault(card));

        card.AbilityDataTrigger("CardPlayed:Cost", ref data);

        var cost = data.Get<ResourceCost>("Cost") ?? [];

        if (ResourceActions.PayCost(card.Owner, cost))
            card.SummonAsCreature(slot);

    }

}
