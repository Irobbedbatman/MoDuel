using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Data.Assembled;
using MoDuel.Players;

namespace DefaultPackage;

/// <summary>
/// Actions for attack logic.
/// </summary>
public static class AttackActions {

    // The amount of time playback is blocked for during attacking.
    public const double ATTACK_DURATION = 300;

    /// <summary>
    /// The default attack behaviour of a card. Attempts to attack the opposing creature. If there is no creature attacks the hero.
    /// </summary>
    [ActionName(nameof(AttackDefault))]
    public static void AttackDefault(CardInstance attacker) {
        var attackerPosition = attacker.FieldPosition;
        if (attackerPosition == null)
            return;

        var opposingPosition = attackerPosition.GetOpposingSlot();
        if (opposingPosition.Occupant == null)
            AttackHero(attacker, opposingPosition.ParentField.Owner);
        else
            AttackCreature(attacker, opposingPosition.Occupant);
    }

    /// <summary>
    /// The behaviour when ac creature attacks another creature.
    /// </summary>
    [ActionName(nameof(AttackCreature))]
    public static void AttackCreature(CardInstance attacker, CardInstance defender) {
        if (!attacker.IsAlive)
            return;
        attacker.Context.SendBlockingRequest(new ClientRequest("AttackCreature", attacker.Index, defender.Index), ATTACK_DURATION);
        DamageActions.InflictDamageToCreature(defender, attacker.Attack);
    }

    /// <summary>
    /// The behaviour when a creature attacks the opposing hero.
    /// </summary>
    [ActionName(nameof(AttackHero))]
    public static void AttackHero(CardInstance attacker, Player defender) {
        if (!attacker.IsAlive)
            return;
        attacker.Context.SendBlockingRequest(new ClientRequest("AttackHero", attacker.Index), ATTACK_DURATION);
        DamageActions.InflictDamageToPlayer(defender, attacker.Attack);
    }

}
