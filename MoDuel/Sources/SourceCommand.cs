using MoDuel.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Sources;

/// <summary>
/// A source from a command sent by a player.
/// </summary>
public class SourceCommand : Source {

    /// <summary>
    /// The command sent.
    /// </summary>
    public readonly string Command;

    /// <summary>
    /// The player who sent the command.
    /// </summary>
    public readonly Player Player;

    public SourceCommand(string command, Player player) {
        Command = command;
        Player = player;
    }

}
