using MoDuel;
using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Players;

namespace DefaultPackage;

/// <summary>
/// Actions to heal players or creatures.
/// </summary>
public static class HealActions {

    /// <summary>
    /// Dynamic action to call <see cref="HealPlayer(Player, int)"/> or <see cref="HealPlayer(Player, int)"/>.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    [ActionName(nameof(Heal))]
    public static void Heal(Target target, int amount) {
        if (target is Player player)
            HealPlayer(player, amount);
        if (target is CreatureInstance creature)
            HealCreature(creature, amount);
    }

    /// <summary>
    /// Heals a creature the provided <paramref name="amount"/> of lie.
    /// </summary>
    [ActionName(nameof(HealCreature))]
    public static void HealCreature(CreatureInstance creature, int amount) {

        creature.Life += amount;
        creature.Life = Math.Clamp(creature.Life, 0, creature.MaxLife);
        creature.Context.SendRequest(new ClientRequest("Heal", creature.Index, amount));
        creature.Context.SendRequest(new ClientRequest("UpdateHealthDisplay", creature.Index, creature.Life));

    }

    /// <summary>
    /// Heals a player the provided <paramref name="amount"/> of life.
    /// </summary>\
    [ActionName(nameof(HealPlayer))]
    public static void HealPlayer(Player player, int amount) {

        player.Life += amount;
        player.Life = Math.Clamp(player.Life, 0, player.MaxLife);
        player.Context.SendRequest(new ClientRequest("Heal", player.Index, amount));
        player.Context.SendRequest(new ClientRequest("UpdateHealthDisplay", player.Index, player.Life));

    }

}
