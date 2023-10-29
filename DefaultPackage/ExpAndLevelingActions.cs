using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Players;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// Actions that provide logic for changing exp and level values.
/// </summary>
public static class ExpAndLevelingActions {

    /// <summary>
    /// The amount of exp that a player needs at each level to level up.
    /// </summary>
    public static readonly Dictionary<int, int> ExpNeededEachLevel = new() {
        {1, 6},
        {2, 30}
    };

    /// <summary>
    /// The check to see if a player has enough exp to level up.
    /// </summary>
    [ActionName(nameof(CanLevelUp))]
    public static bool CanLevelUp(this Player player) {
        if (ExpNeededEachLevel.TryGetValue(player.Level, out var nextExp)) {
            return player.Exp == nextExp;
        }
        return false;
    }

    /// <summary>
    /// The action to proivde an <paramref name="amount"/> of experince to a player. This cannot go above the required for the next level.
    /// </summary>
    [ActionName("GainExp")]
    public static void GainExp(Player player, int amount) {
        if (amount < 1) {
            GlobalActions.Fizzle();
            return;
        }

        if (!ExpNeededEachLevel.TryGetValue(player.Level, out int nextExp)) {
            return;
        }

        player.Exp = Math.Clamp(player.Exp + amount, 0, nextExp);
        player.Context.SendRequest(new ClientRequest("GainExmp", player.Index, player.Exp));

    }

    /// <summary>
    /// The action that will level up the player if they have enough exp.
    /// </summary>
    [ActionName(nameof(LevelUp))]
    public static void LevelUp(Player player) {
        if (!ExpNeededEachLevel.TryGetValue(player.Level, out int nextExp) || nextExp > player.Exp) {
            return;
        }

        player.Level += 1;
        player.Exp = 0;

        var oldLife = player.Life;
        player.Life = (int)MathF.Ceiling(oldLife * 1.5f);
        var difference = player.Life - oldLife;
        player.MaxLife += difference;

        ResourceActions.GainEachResource(player);

        player.Context.SendRequest(new ClientRequest("LevelUp", player));
        player.Context.SendRequest(new ClientRequest("PlayerHpChange", player, player.Life, player.MaxLife));

        player.Context.Trigger("LeveledUp", player);

    }


}
