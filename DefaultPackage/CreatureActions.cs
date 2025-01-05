using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Field;

namespace DefaultPackage;

/// <summary>
/// Actions that are used by creatures.
/// </summary>
public static class CreatureActions {

    /// <summary>
    /// Sets the position of the creature on the field to summon it.
    /// </summary>
    [ActionName(nameof(Summon))]
    public static void Summon(this CardInstance creature, FieldSlot location) {
        creature.Position = location;
        creature.Values[CombatActions.SUMMONING_SICKNESS] = true;
        // TODO CLIENT: Summon
    }


    [ActionName(nameof(Unsummon))]
    public static void Unsummon(this CardInstance creature) {
        // TODO DELAY: Unsummon
    }


    public static void MoveCreature(this CardInstance creature, FieldSlot newLocation) {
        creature.Position = newLocation;
        // TODO DELAY: Move Creature.
    }

    /// <summary>
    /// Kills the creature. The creatures original state will be sent to the grave.
    /// </summary>
    [ActionName("KillCreature")]
    public static void Kill(this CardInstance creature) {
        creature.Position = null;
        var card = creature.OriginalState;
        card.MoveCardToGrave(card.TrueOwner);
        // TODO DELAY: Kill creature exp.
        // TODO CLIENT: Kill creature
    }



}
