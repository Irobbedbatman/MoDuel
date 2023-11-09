using MoDuel.Players;

namespace MoDuel.Flow;

/// <summary>
/// A key used by <see cref="DuelFlow.CommandBuffer"/>.
/// <para>Ordered by the <see cref="Time"/> they were received.</para>
/// <para>Takes account of the <see cref="DuelFlow.SysPlayer"/>; which has priority.</para>
/// </summary>
public readonly struct CommandReference(Player player, DateTime time) : IComparable<CommandReference> {

    /// <summary>
    /// The player that sent the command.
    /// </summary>
    public readonly Player Player = player;
    /// <summary>
    /// The time the command was received.
    /// </summary>
    public readonly DateTime Time = time;

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
    /// Equate commands sent from the same player as being equivalent.
    /// <para>System players are always considered unequal.</para>
    /// </summary>
    public override readonly bool Equals(object? obj) {

        // System players are always considered unequal.
        if (Player.Equals(Player.SysPlayer))
            return false;

        // Ensure other object is a command.
        if (obj is CommandReference cmdRef) {
            return Player.Equals(cmdRef.Player);
        }
        return false;
    }

    /// <summary>
    /// Hash code logic that aligns with the behaviour found in <see cref="Equals(object)"/>
    /// </summary>
    public override readonly int GetHashCode() {
        // System players are always considered unequal.
        if (Player.Equals(Player.SysPlayer))
            return base.GetHashCode();
        // Derive hash code from the player.
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
