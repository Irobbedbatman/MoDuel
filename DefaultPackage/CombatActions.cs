using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// Actions that define the combat phase.
/// </summary>
public static class CombatActions {


    /// <summary>
    /// The value stored on the state to list all creatures that can attack this turn.
    /// </summary>
    public const string READY_CREATURES = "CreaturesReadyToAttack";

    /// <summary>
    /// The value stored on creatures that indicates they were summoned and cannot attack this turn.
    /// </summary>
    public const string SUMMONING_SICKNESS = "SummonSickness";

    /// <summary>
    /// Start the combat phase and cause each creature to attack.
    /// </summary>
    [ActionName(nameof(ExecuteCombatPhase))]
    public static void ExecuteCombatPhase(DuelState state) {

        // Get the ready list of creatures on the duel state.s
        ReadyAttackers(state);

        // The comparer that will inform the order of attackers.
        var comparer = new CardInstanceComparer(state.CurrentTurn.Owner, "AttackCompare");

        /// The hashset on the state that stores attack data.
        var attackers = (HashSet<CardInstance>?)state.Values[READY_CREATURES];

        // Have each attacker attack.
        while (attackers?.Count > 0) {
            var attacker = GetNextAttacker(state, comparer);
            attacker?.FallbackTrigger("Attack", AttackActions.AttackDefault);
            if (attacker == null)
                break;
            attackers.Remove(attacker);
        }

    }

    /// <summary>
    /// The default check on a creature to determine if the creature can attack this combat phase.
    /// </summary>
    [ActionName(nameof(DefaultCreatureReadyCheck))]
    public static bool DefaultCreatureReadyCheck(CardInstance creature, DuelState state) {
        if (!creature.IsAlive || creature.Owner != state.CurrentTurn.Owner || creature.Imprint.HasTag("Pacifist")) {
            return false;
        }
        dynamic sick = creature.Values.GetValueOrDefault(SUMMONING_SICKNESS, false) ?? false;
        return !sick;
    }

    /// <summary>
    /// Store the set of all creatures on the field that can attack this turn.
    /// </summary>
    public static void ReadyAttackers(DuelState state) {

        var eligableAttacks = state.Field.GetCreatures().Where(
            (creature) => {
                return creature.FallbackTrigger("ReadyToAttack", DefaultCreatureReadyCheck,
                    state);
            }).ToHashSet();

        // TODO CLIENT: Effects.

        state.Values[READY_CREATURES] = eligableAttacks;
    }

    /// <summary>
    /// Get the creature that will perform the next attack.
    /// </summary>
    public static CardInstance? GetNextAttacker(DuelState state, CardInstanceComparer comparer) {
#nullable disable
        var creatures = (HashSet<CardInstance>)state.Values.GetValueOrDefault(READY_CREATURES, new HashSet<CardInstance>());
        foreach (var creature in creatures.ToArray()) {
            if (!creature.FallbackTrigger(
                "ReadyToAttack",
                DefaultCreatureReadyCheck,
                state)) {
                creatures.Remove(creature);
            }
        }
        return (CardInstance)creatures.Order(comparer).FirstOrDefault();
#nullable enable
    }


}
