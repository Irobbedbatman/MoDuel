using MoDuel.Cards;
using MoDuel.Data.Assembled;
using MoDuel.Field;
using MoDuel.Resources;
using MoDuel.Shared.Json;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.Triggers;

namespace DefaultPackage;

/// <summary>
/// Actions that apply to <see cref="Card"/>s and <see cref="CardInstance"/>s.
/// </summary>
public static partial class CardActions {

    /// <summary>
    /// Creates a new creature based on the <paramref name="card"/> and place in <paramref name="position"/>.
    /// </summary>
    /// <param name="card">The card to summon as a creature.</param>
    /// <param name="position">The slot to summon the creature into.</param>
    [ActionName(nameof(SummonAsCreature))]
    public static CardInstance SummonAsCreature(this CardInstance card, FieldSlot position) {

        var source = new SourceEntity(card);
        var state = card.Context;

        // Both trigger will use the following data.
        DataTable data = new() {
            ["Card"] = card,
            ["Level"] = card.GetLevelDefault()
        };

        // First get the level of the card.

        var trigger = new Trigger("Card:Summoned:Level", source, state, TriggerType.ExplicitDataOverride);
        state.ExplicitDataTrigger(trigger, ref data);

        card.Level = data.Get<int?>("Level") ?? 1;

        // Get the other stats on the card.

        data["Attack"] = GetAttackDefault(card, card.Level);
        data["Armour"] = GetArmourDefault(card, card.Level);
        data["MaxLife"] = GetMaxLifeDefault(card, card.Level);
        data["Life"] = data["MaxLife"];

        trigger = new Trigger("Card:Summoned:Stats", source, state, TriggerType.ExplicitDataOverride);
        state.ExplicitDataTrigger(trigger, ref data);

        card.Attack = data.Get<int?>("Attack") ?? 0;
        card.Armour = data.Get<int?>("Armour") ?? 0;
        card.Defence = data.Get<int?>("Defence") ?? 0;
        card.MaxLife = data.Get<int?>("MaxLife") ?? 1;
        card.Life = data.Get<int?>("Life") ?? 1;

        // TODO: consider clamping life.

        card.Summon(position);

        return card;

        // TODO CLIENT: animations
    }

    /// <summary>
    /// Read the cost from the json data at the <paramref name="level"/> provided.
    /// </summary>
    [ActionName(nameof(GetLevelledCost))]
    public static ResourceCost GetLevelledCost(this Card card, int level) {
        var costToken = card.GetLevelledParameter("Cost", level);
        return new ResourceCost(card.Package, costToken);
    }

    /// <summary>
    /// The default check to see if a card can be revived. THis is always true.
    /// </summary>
    [ActionName(nameof(CanBeRevivedDefault))]
    public static bool CanBeRevivedDefault(CardInstance _) => true;

    /// <summary>
    /// The default action to revive a card to hand.
    /// </summary>
    /// <param name="card"></param>
    [ActionName(nameof(ReviveDefault))]
    public static void ReviveDefault(CardInstance card) {
        card.MoveCardToHand(card.TrueOwner);
    }

}
