using MoDuel.Data;
using MoDuel.Players;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// Actions that define the nature of duel turns.
/// </summary>
public static class TurnActions {


    /// <summary>
    /// Action used to chaneg the turn to the next next players.
    /// </summary>
    [ActionName(nameof(ChangeTurn))]
    public static void ChangeTurn(DuelState state) {

        var turnPlayer = state.CurrentTurn.Owner;
        var opposing = state.GetOpposingPlayer(turnPlayer);
        state.NewTurn(opposing);
        opposing.ActionPoints = opposing.Level;
        StartTurn(state);

    }

    /// <summary>
    /// Action used to start a turn including any setup.
    /// </summary>
    [ActionName("StartTurn")]
    public static void StartTurn(DuelState state) {
        Player player = state.CurrentTurn.Owner;
        ResourceActions.GainEachResource(player);
        WipeSummoningSickness(player);
    }

    /// <summary>
    /// Action used to wipe summoning sickness from all the creatures a player controls.
    /// </summary>
    public static void WipeSummoningSickness(Player player) {
        foreach (var creature in player.Field.GetCreatures()) {
            creature.Values[CombatActions.SUMMONING_SICKNESS] = false;
        }
    }

}
