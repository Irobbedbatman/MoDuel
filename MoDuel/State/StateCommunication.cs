using MoDuel.Client;
using MoDuel.Flow;
using MoDuel.Players;
using MoDuel.Tools;

namespace MoDuel.State;

// Look at DuelState.cs for documentation.
public partial class DuelState {

    /// <summary>
    /// The delegate used to send out broadcast messages.
    /// </summary>
    public EventHandler<ClientRequest> OutBoundDelegate = delegate { };

    /// <summary>
    /// Invokes <see cref="OutBoundDelegate"/> with a request for the client to do.
    /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>/// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    public void SendRequest(ClientRequest request) {
        LogSettings.LogEvent("SendRequest [" + request.RequestId + "]", LogSettings.LogEvents.OutboundRequests);
        OutBoundDelegate?.Invoke(this, request);
    }

    /// <summary>
    /// Invokes <see cref="Player.OutBoundDelegate"/> with a request for the client to do.
    /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>
    /// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    public static void SendRequestTo(Player target, ClientRequest request) {
        LogSettings.LogEvent("SendRequest [" + request.RequestId + "] to [" + target.UserId + "]", LogSettings.LogEvents.OutboundRequests);
        target.SendRequest(request);
    }

    /// <summary>
    /// Blocks the execution of the current <see cref="Thread"/>.
    /// </summary>
    /// <param name="blockDuration">How long to block the thread by, affected by <see cref="DuelSettingsLoaded.BlockPlaybackDurationMultiplier"/></param>
    public void BlockPlayback(double blockDuration) {
        var settings = Settings;
        if (!settings.IsPlaybackBlocked) {
            PlaybackBlockingHandler blocker = new();
            blocker.StartBlock(blockDuration * settings.BlockPlaybackDurationMultiplier);
        }
    }

    /// <summary>
    /// Sends a request to all listeners of the <see cref="OutBoundDelegate"/> waiting for a ready response from the two players found in the <see cref="State"/>.
    /// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    /// <param name="timeout">An amount of time in milliseconds that the blocking will finish and playback will resume.</param>
    /// <returns>A tuple with three values.
    /// <list type="number">
    /// <item>True if if both players are ready, false otherwise.</item>
    /// <item>True if Player1 has responded ready.</item>
    /// <item>True if Player2 has responded ready.</item>
    /// </list>
    /// </returns>
    public (bool, bool, bool) SendBlockingRequest(ClientRequest request, double timeout) {

        LogSettings.LogEvent("SendBlockingRequest [" + request.RequestId + "]", LogSettings.LogEvents.OutboundRequests);

        // If there is no playback blocking skip the expensive operations.
        if (Settings.IsPlaybackBlocked) {
            SendRequest(request);
            return (true, true, true);
        }

        // Create the blocker that will be used.
        PlaybackBlockingHandler blocker = new();
        bool player1Ready = false;
        bool player2Ready = false;
        // Thread safe lock for the events.

        // Create the read event, that will determine that both players are read.y
        object readyLock = new();
        Player1.InBoundReadDelegate = delegate {
            lock (readyLock) {
                // Set this player is ready.
                player1Ready = true;
                // If both players are ready playback can stop.
                if (player2Ready) {
                    blocker.EndBlock();
                }
            }
            Player1.InBoundReadDelegate = delegate { };
        };
        Player2.InBoundReadDelegate = delegate {
            lock (readyLock) {
                // Set this player is ready.
                player2Ready = true;
                // If both players are ready playback can stop.
                if (player1Ready) {
                    blocker.EndBlock();
                }
            }
            Player2.InBoundReadDelegate = delegate { };
        };

        // Send the request to the players.
        OutBoundDelegate?.Invoke(this, request.WithReadyConfirmation());
        var timedOut = blocker.StartBlock(timeout * Settings.BlockPlaybackDurationMultiplier);
        // After the block has finished no longer need to listen to the delegate.
        Player1.InBoundReadDelegate = delegate { };
        Player2.InBoundReadDelegate = delegate { };
        return (!timedOut, player1Ready, player2Ready);
    }



}
