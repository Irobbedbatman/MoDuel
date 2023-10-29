using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Field;

namespace DefaultPackage;

/// <summary>
/// Actions that are used by creatures.
/// </summary>
public static class CreatureActions {

    /// <summary>
    /// The default life a creature will be summoned with. The default is the creatures max life.
    /// </summary>
    [ActionName(nameof(GetBaseLifeDefault))]
    public static int GetBaseLifeDefault(CreatureInstance creature) {
        return creature.MaxLife;
    }

    /// <summary>
    /// Sets the position of the creature on the field to summon it.
    /// </summary>
    [ActionName(nameof(Summon))]
    public static void Summon(this CreatureInstance creature, FieldSlot location) {
        creature.Position = location;
        creature.Values[CombatActions.SUMMONING_SICKNESS] = true;
        // TODO CLIENT: SUmmon
    }


    [ActionName(nameof(Unsummon))]
    public static void Unsummon(this CreatureInstance creature) {
        // TODO DELAY: Unsummon
    }


    public static void MoveCreature(this CreatureInstance creature, FieldSlot newLocation) {
        creature.Position = newLocation;
        // TODO DELAY: Move Creature.
    }

    /// <summary>
    /// Kills the creauture. The creatures original state will be sent to the grave.
    /// </summary>
    [ActionName("KillCreature")]
    public static void Kill(this CreatureInstance creature) {
        creature.Position = null;
        var card = creature.OriginalState;
        card.MoveCardToGrave(card.OriginalOwner);
        // TODO DELAY: Kill creature exp.
        // TODO CLIENT: Kill creature
    }



}
