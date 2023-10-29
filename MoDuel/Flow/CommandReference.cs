using MoDuel.Players;

namespace MoDuel.Flow;

/// <summary>
/// A key used by <see cref="DuelFlow.CommandBuffer"/>.
/// <para>Ordered by the <see cref="Time"/> they were recieved.</para>
/// <para>Takes account of the <see cref="DuelFlow.SysPlayer"/>; which has priority.</para>
/// </summary>
public readonly struct CommandReference : IComparable<CommandReference> {

    /// <summary>
    /// The player that sent the command.
    /// </summary>
    public readonly Player Player;
    /// <summary>
    /// The time the command was recieved.
    /// </summary>
    public readonly DateTime Time;

    public CommandReference(Player player, DateTime time) {
        Player = player;
        Time = time;
    }

    public readonly int CompareTo(CommandReference cmdRef) {

        // Prioritise the system player.
        if (Player == Player.SysPlayer && cmdRef.Player != Player.SysPlayer) {
            return -1;
        }
        // Prioritise the system player.
        if (Player != Player.SysPlayer && cmdRef.Player == Player.SysPlayer) {
            return 1;
        }

        // Compare the remaining time on both commands. Prioritising the one that was created first.
        return Time.CompareTo(cmdRef.Time);
    }


    /// <summary>
    /// Equate commands sent from the same player as being equivalant.
    /// <para>System players are always considered uneuqal.</para>
    /// </summary>
    public override readonly bool Equals(object? obj) {

        // System players are always considered uneuqal.
        if (Player.Equals(Player.SysPlayer))
            return false;

        // Ensure other object is a command.
        if (obj is CommandReference cmdRef) {
            return Player.Equals(cmdRef.Player);
        }
        return false;
    }

    /// <summary>
    /// Hashcode logic that alligns with the behaviour found in <see cref="Equals(object)"/>
    /// </summary>
    public override readonly int GetHashCode() {
        // System players are always considered uneuqal.
        if (Player.Equals(Player.SysPlayer))
            return base.GetHashCode();
        // Derive hashcode from the player.
        return Player.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator <(CommandReference left, CommandReference right) {
        return left.CompareTo(right) < 0;
    }

    /// <inheritdoc/>
    public static bool operator >(CommandReference left, CommandReference right) {
        return left.CompareTo(right) > 0;
    }

    /// <inheritdoc/>
    public static bool operator <=(CommandReference left, CommandReference right) {
        return left.CompareTo(right) <= 0;
    }

    /// <inheritdoc/>
    public static bool operator >=(CommandReference left, CommandReference right) {
        return left.CompareTo(right) >= 0;
    }

    /// <inheritdoc/>
    public static bool operator ==(CommandReference left, CommandReference right) {
        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(CommandReference left, CommandReference right) {
        return !(left == right);
    }
}
