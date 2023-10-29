using MoDuel.Flow;
using MoDuel.Players;

namespace Testing;

/// <summary>
/// The command parser and enqueuer.
/// </summary>
public static class CommandReader {

    /// <summary>
    /// Parse the <paramref name="command"/> and sends a new command to the <paramref name="flow"/>.
    /// </summary>
    /// <param name="flow">The deul flow that will use the command.</param>
    /// <param name="player">The player that will send the command.</param>
    /// <param name="command">The un-parsed command read from user.</param>
    /// <returns>True if the duel should keep going. False if the application should terminate.</returns>
    public static bool ReadCommand(DuelFlow flow, Player player, string command) {

        string[] split = command.Split(' ');

        // Read the actual command.
        switch (split[0].ToLower()) {
            case "charge":
            case "m":
            case "meditate":
            case "med":
                flow.EnqueueCommand(player, "CmdMeditate");
                break;
            case "endturn":
            case "end":
                flow.EnqueueCommand(player, "CmdEndTurn");
                break;
            case "lvl":
            case "level":
            case "lvlup":
            case "levelup":
                flow.EnqueueCommand(player, "CmdLevelUp");
                break;
            case "rev":
            case "revive":
                flow.EnqueueCommand(player, "CmdRevive");
                break;
            case "disc":
            case "discard":
                var dicard = GetTargetIndex(split, 1);
                flow.EnqueueCommand(player, "CmdDiscard", dicard);
                break;
            case "play":
                var playcard = GetTargetIndex(split, 1);
                var target = GetTargetIndex(split, 2);
                flow.EnqueueCommand(player, "CmdPlayCard", playcard, target);
                break;
            case "hand":
                Display.Collection("Your Hand", player.Hand);
                break;
            case "grave":
                Display.Collection("Your Grave", player.Grave);
                break;
            case "quit":
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets a numeric value from a split string. The <paramref name="arrayIndex"/> in the <paramref name="args"/> will be parsed.
    /// </summary>
    /// <param name="args">The split string that contains arguments.</param>
    /// <param name="arrayIndex">The index in <paramref name="args"/> to read.</param>
    /// <returns>The int read or null if it couldn't be read.</returns>
    public static int? GetTargetIndex(string[] args, int arrayIndex) {
        if (arrayIndex >= args.Length)
            return null;
        return int.TryParse(args[arrayIndex], out int value) ? value : null;
    }

}
