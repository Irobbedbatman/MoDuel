using MoDuel.Cards;
using MoDuel.Field;
using MoDuel.OngoingEffects;
using MoDuel.Players;
using MoDuel.Time;

namespace MoDuel.State;


// Look at DuelState.cs for documentation.
public partial class DuelState {

    /// <summary>
    /// One of the two players that are taking part in the game.
    /// </summary>
    public readonly Player Player1, Player2;

    /// <summary>
    /// The combination <see cref="Field"/ > of <see cref="SubField"/>s owned <see cref="Player1"/> and <seealso cref="Player2"/>
    /// </summary>
    public readonly FullField Field;

    /// <summary>
    /// The manager for all the targets within the state.
    /// </summary>
    public readonly TargetRegistry TargetRegistry = new(new());

    /// <summary>
    /// The <see cref="CardInstance"/>s that are enabled for triggering effects.
    /// </summary>
    public readonly CardInstanceManager CardManager = new();

    /// <summary>
    /// The manager that stores all the <see cref="OngoingEffect"/>s and allows for compartmentalised functionality.
    /// </summary>
    public readonly OngoingEffectManager EffectManager = new();

    /// <summary>
    /// Stops the timers for both <see cref="Player1"/> and <see cref="Player2"/>.
    /// </summary>
    public void StopAllTimers() {
        Player1.Timer?.Stop();
        Player2.Timer?.Stop();
    }

    /// <summary>
    /// Assigns <see cref="PlayerTimers"/> for both <see cref="Player1"/> and <see cref="Player2"/>.
    /// </summary>
    /// <param name="settings"The <see cref="TimerSettings"/> both timers will use.></param>
    /// <param name="threadLock">The lock object to handle timeouts in a threadsafe way,</param>
    public void AssignTimers(TimerSettings settings, object threadLock) {
        Player1.AssignTimer(settings, threadLock);
        Player2.AssignTimer(settings, threadLock);
    }

    /// <summary>
    /// Gets the player from their <see cref="Player.UserId"/>.
    /// <para>If an invalid <paramref name="userId"/> is provided null is returned.</para>
    /// </summary>
    /// <param name="userId">The <see cref="Player.UserId"/> of a player to check.</param>
    public Player? GetPlayerByUserID(string userId) {
        if (Player1.UserId == userId)
            return Player1;
        else if (Player2.UserId == userId)
            return Player2;
        else
            return null;
    }

    /// <summary>
    /// Gets the other player to the provided player.
    /// <para>If an invalid <paramref name="player"/> is provided null is returned.</para>
    /// </summary>
    public Player GetOpposingPlayer(Player player) {
        if (player == Player1)
            return Player2;
        else if (player == Player2)
            return Player1;
        else
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
    }

}
