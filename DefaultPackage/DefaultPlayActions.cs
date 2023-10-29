using MoDuel;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Field;
using MoDuel.Resources;

namespace DefaultPackage;

/// <summary>
/// The default actions that occurwhen a card is played based on its type.
/// </summary>
public static class DefaultPlayActions {

    /// <summary>
    /// The generic play card action that will branch to ther other play card actions based on the card's type.s
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
    public static void PlayCardCreature(CardInstance cardInstance, Target target) {

        if (target is not FieldSlot slot || slot.IsOccupied || slot.ParentField.Owner != cardInstance.Owner)
            return;

        // Get the level and then the cost of the card at that level.
        var level = cardInstance.FallbackTrigger("GetLevel", CardActions.GetLevelDefault);
        var cost = cardInstance.FallbackTrigger("GetCost", new ActionFunction(CardActions.GetCostDefault), level);

        if (ResourceActions.PayCost(cardInstance.Owner, cost))
            cardInstance.SummonAsCreature(slot);

    }

}
