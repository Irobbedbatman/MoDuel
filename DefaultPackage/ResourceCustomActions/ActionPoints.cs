using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Players;
using MoDuel.Resources;

namespace DefaultPackage.ResourceCustomActions;

/// <summary>
/// Actions that are create the unique behaviour of action points.
/// </summary>
public static class ActionPoints {

    /// <summary>
    /// Check to see if a player has more or equal action points when compared to the provided <paramref name="amount"/>.
    /// </summary>
    [ActionName(nameof(CanPayActionPoints))]
    public static bool CanPayActionPoints(ResourceType _, Player player, int amount) {
        return player.ActionPoints >= amount;
    }

    /// <summary>
    /// The provided <paramref name="player"/> pays the provided <paramref name="amount"/> of action points.
    /// </summary>
    [ActionName(nameof(PayActionPoints))]
    public static void PayActionPoints(ResourceType _, Player player, int amount) {
        player.ActionPoints -= amount;
        var state = player.Context;
        state.SendRequest(new ClientRequest("ConsumedActionPoint", amount));
        state.Trigger("ConsumedActionPoint", amount);
        ExpAndLevelingActions.GainExp(player, 2);
    }

}
