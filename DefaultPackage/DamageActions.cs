using MoDuel;
using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Players;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// Actions used to inflic or calculate damage dealt.
/// </summary>
public static class DamageActions {

    /// <summary>
    /// The amount of exp an oppising player gets when a player takes damage.
    /// </summary>
    public const int INFLICT_DAMAGE_TO_PLAYER_EXP = 1;

    /// <summary>
    /// The amount of exp an oppising player gets when a creature is killed.
    /// </summary>
    public const int KILL_CREATURE_EXP = 1;


    /// <summary>
    /// Resets the per command flag on wether a creature was killed the last command.
    /// </summary>
    [ActionName("ResetKilledFlag")]
    public static void ResetCreatureKilledFlag(DuelState context) {
        context.Player1.Values["KilledCreature"] = false;
        context.Player2.Values["KilledCreature"] = false;
    }


    /// <summary>
    /// Inflicts the provided amount of <paramref name="damage"/> to the provided <paramref name="target"/>.
    /// <para>This will call the similarly named actions if the target is a creature or a player.</para>
    /// </summary>
    /// <param name="damage">The damage to deal.</param>
    /// <param name="lethal">Can the damage reduce the targets life below 1.</param>
    [ActionName(nameof(InflictDamage))]
    public static void InflictDamage(Target target, int damage, bool lethal = true) {

        if (damage < 0) {
            GlobalActions.Fizzle();
            return;
        }

        if (target is Player player)
            InflictDamageToPlayer(player, damage, lethal);
        if (target is CardInstance creature) {
            InflictDamageToCreature(creature, damage, lethal);
        }
    }

    /// <summary>
    /// Inflicts the provided amount of <paramref name="damage"/> to the provided <paramref name="player"/>.
    /// </summary>
    /// <param name="damage">The damge to deal.</param>
    /// <param name="lethal">Can the damage reduce the targets life below 1.</param>
    public static void InflictDamageToPlayer(Player player, int damage, bool lethal = true) {

        player.Life -= damage;
        player.Life = Math.Max(player.Life, lethal ? 0 : 1);

        player.Context.SendRequest(new ClientRequest("InflictDamage", player.Index, damage));
        player.Context.SendRequest(new ClientRequest("UpdateHealthDisplay", player.Index, player.Life));

        if (player.Life <= 0) {
            player.Kill();
            return;
        }

        var opposer = player.Context.GetOpposingPlayer(player);
        ExpAndLevellingActions.GainExp(opposer, INFLICT_DAMAGE_TO_PLAYER_EXP);

    }

    /// <summary>
    /// Calculates the amount of damage that will be done after armour reduction.
    /// </summary>
    public static int DamageToArmourCalculation(CardInstance target, int damage) {
        var def = target.Armour;
        if (def - damage <= 0) {
            target.Armour = 0;
            return damage - def;
        }

        // TODO CLIENT: Armour damage effects.

        // Get the new value of armor. That is 100% damage effectivness to only 1 damage.
        var rand = target.Context.Random.Next(def - damage, def);
        target.Armour = rand;
        return 0;
    }

    /// <summary>
    /// Inflicts the provided amount of <paramref name="damage"/> to the provided <paramref name="creature"/>.
    /// </summary>
    /// <param name="damage">The damage to deal.</param>
    /// <param name="lethal">Can the damage reduce the targets life below 1.</param>

    public static void InflictDamageToCreature(CardInstance creature, int damage, bool lethal = true, bool ignoreArmor = false) {

        if (!ignoreArmor)
            damage = DamageToArmourCalculation(creature, damage);

        creature.Life -= damage;
        creature.Life = Math.Max(creature.Life, lethal ? 0 : 1);

        creature.Context.SendRequest(new ClientRequest("InflictDamage", creature.Index, damage));
        creature.Context.SendRequest(new ClientRequest("UpdateHealthDisplay", creature.Index, creature.Life));

        if (creature.Life <= 0) {
            creature.Kill();
        }
    }

}
