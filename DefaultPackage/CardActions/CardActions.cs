using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Field;
using MoDuel.Json;
using MoDuel.Resources;
using MoDuel.Triggers;

namespace DefaultPackage;

/// <summary>
/// Actions that apply to <see cref="Card"/>s and <see cref="CardInstance"/>s.
/// </summary>
public static partial class CardActions {

    /// <summary>
    /// Creates a new creature based on the <paramref name="card"/> and place in <paramref name="position"/>.
    /// </summary>
    /// <param name="card">The card to summon as a creture.</param>
    /// <param name="position">The slot to summon the creatyre into.</param>
    [ActionName(nameof(SummonAsCreature))]
    public static CreatureInstance SummonAsCreature(this CardInstance card, FieldSlot position) {

        CreatureInstance creature = new(card) {
            Level = card.FallbackTrigger("GetLevel", new MoDuel.ActionFunction(CardActions.GetLevelDefault)),
        };

        creature.Attack = card.FallbackTrigger("GetAttack", new MoDuel.ActionFunction(CardActions.GetAttackDefault), creature.Level);
        creature.Armour = card.FallbackTrigger("GetArmour", new MoDuel.ActionFunction(CardActions.GetArmourDefault), creature.Level);
        creature.MaxLife = card.FallbackTrigger("GetMaxLife", new MoDuel.ActionFunction(CardActions.GetMaxLifeDefault), creature.Level);
        creature.Life = creature.FallbackTrigger("GetBaseLife", new MoDuel.ActionFunction(CreatureActions.GetBaseLifeDefault));

        creature.Summon(position);

        creature.Values["Jason"] = creature.Imprint.Data;
        
        return creature;

        // TODO CLIENT: animations
    }

    /// <summary>
    /// Read the cost from the json data at the <paramref name="level"/> provided.
    /// </summary>
    [ActionName(nameof(GetLeveledCost))]
    public static ResourceCost GetLeveledCost(this Card card, int level) {
        return ResourceActions.ParseTokenToCost(card.Data.TryGet("Cost"), card.Package, level);
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
        card.MoveCardToHand(card.OriginalOwner);
    }

}
