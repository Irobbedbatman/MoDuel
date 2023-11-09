using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Flow;
using MoDuel.Players;
using MoDuel.Tools;
using MoDuel.Triggers;

namespace MoDuel.State;


// Look at DuelState.cs for documentation.
public partial class DuelState {

    /*
    This details some of way triggers and how args are passed around.
    triggerable, [args...]
    ----
    CMD
    > player, args...
    ---
    IExplicitTriggerable.(Fallback)Trigger
    > triggerable, args...
    ----
    StateGetIImplicitTriggerables.Call
    > reactor, args...
    Incite
    > reactor, inciter, args..., 
    Overwrite
    > reactor, overwriteTable
    ----
    Comparer.Call
    > comparer, object1, object2
    ----
    TimeOutAction.Call
    > Player
    */


    /// <summary>
    /// How many triggers have activated sequentially.
    /// </summary>
    private float currentTriggerChain = 1;

    /// <summary>
    /// How many triggers can be activated depth wise. Used to dodge infinite loops.
    /// <para>Calling the same action within itself is one example of a endless loop unaffected.</para>
    /// </summary>
    public const float MAX_TRIGGER_CHAIN = 22;

    /// <summary>
    /// Checks and returns any reactions to the given <paramref name="trigger"/>.
    /// <para>Checks their hero abilities, grave triggers, hand triggers and alive triggers in order.</para>
    /// <para>Each type of reaction happens for both players before going to the next with the turn players first.</para>
    /// </summary>
    /// <param name="trigger">The trigger keyword.</param>
    /// <returns>A list of pairs with the keys being the sender and the values are the reaction functions that should be run in sequence.</returns> 
    public IEnumerable<KeyValuePair<object, ActionFunction>> FindReactions(string trigger) {

        // Reactions should only occur during the duel.
        if (!Ongoing)
            return Array.Empty<KeyValuePair<object, ActionFunction>>();

        //Get the two players.
        var owner = CurrentTurn.Owner;
        var opposer = GetOpposingPlayer(owner);

        // Ensure both players are valid.
        if (owner == null || opposer == null) {
            return Array.Empty<KeyValuePair<object, ActionFunction>>();
        }

        // The ordered set of triggerers to get reactions from.
        SortedSet<IImplicitTriggerable> reactors = new(new TriggererPriorityComparer(owner, trigger));

        // Add both players heroes as reactors if they have a reaction.
        if (owner.Hero.HasImplicitReaction(trigger))
            reactors.Add(owner.Hero);
        if (opposer.Hero.HasImplicitReaction(trigger))
            reactors.Add(opposer.Hero);

        // Get the card instance reactors.
        reactors.UnionWith(CardManager.GetOrderedReactors(new CardInstanceComparer(owner, trigger), trigger));

        // Add all the ongoing effects that will react to the trigger.
        foreach (var effect in EffectManager) {
            if (effect.HasImplicitReaction(trigger)) {
                reactors.Add(effect);
            }
        }

        // Return all the reactors paired with their reaction.
        return reactors.Select(
            (reactor) => {
                return new KeyValuePair<object, ActionFunction>(reactor, reactor.GetImplicitReaction(trigger));
            }
        );

    }

    /// <summary>
    /// Triggers reactions found in <see cref="FindReactions(string)"/>
    /// <para>Use <see cref="InciteTrigger(object, string, object[])"/> if you need to provide the reason the trigger occurred.</para>
    /// </summary>
    public void Trigger(string trigger, params object?[] arguments) {
        //Ensure that the game is ongoing.
        if (Finished)
            return;
        //Ensure we don't go above the maximum trigger amount.
        if (currentTriggerChain >= MAX_TRIGGER_CHAIN)
            return;
        currentTriggerChain++;
        foreach (var reaction in FindReactions(trigger)) {
            reaction.Value.Call(arguments.Prepend(reaction.Key).ToArray());
            // Stop triggering if the game is finished.
            if (Finished)
                break;
        }
        currentTriggerChain--;
    }

    /// <summary>
    /// Triggers reactions found in <see cref="FindReactions(string)"/>
    /// <para>Inciting a trigger rather than using <see cref="Trigger(string, object[])"/> forces you to pass the <paramref name="inciter"/>.</para>
    /// </summary>
    public void InciteTrigger(object inciter, string trigger, params object?[] arguments) {
        Trigger(trigger, arguments.Prepend(inciter).ToArray());
    }

    /// <summary>
    /// A trigger that is called so that outcomes can change for specific actions.
    /// <para>Should not be used to perform game state changes only changes to the outcome of the action that trigger it.</para>
    /// <para>Reactions received in <see cref="FindReactions(string)"/> are reversed so that higher priority triggerers have stronger impact.</para>
    /// </summary>
    /// <param name="values">The table that is parsed by reference so that things can change.</param>
    public void OverwriteTrigger(string trigger, Dictionary<string, object?> values) {
        if (DuelFlow.LoggingEnabled)
            Console.WriteLine("OverwriteTrigger [" + trigger + "]");
        //Ensure that the game is ongoing.
        if (!NotFinished)
            return;
        foreach (var reaction in FindReactions(trigger).Reverse()) {
            reaction.Value.Call(reaction.Key, values);
            // Stop triggering if the game is finished.
            if (Finished)
                break;
        }
    }

    /// <summary>
    /// Invokes <see cref="OutBoundDelegate"/> with a request for the client to do.
    /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>/// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    public void SendRequest(ClientRequest request) {
        if (DuelFlow.LoggingEnabled)
            Console.WriteLine("SendRequest [" + request.RequestId + "]");
        OutBoundDelegate?.Invoke(this, request);
    }

    /// <summary>
    /// Invokes <see cref="Player.OutBoundDelegate"/> with a request for the client to do.
    /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>
    /// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    public static void SendRequestTo(Player target, ClientRequest request) {
        if (DuelFlow.LoggingEnabled)
            Console.WriteLine("SendRequest [" + request.RequestId + "] to [" + target.UserId + "]");
        target.SendRequest(request);
    }

    /// <summary>
    /// Blocks the execution of the current <see cref="Thread"/>.
    /// </summary>
    /// <param name="blockDuration">How long to block the thread by, affected by <see cref="DuelSettings.BlockPlaybackDurationMultiplier"/></param>
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
    /// <item>True if timeout occurred; false if both players are ready.</item>
    /// <item>True if Player1 has responded ready.</item>
    /// <item>True if Player2 has responded ready.</item>
    /// </list>
    /// </returns>
    public (bool, bool, bool) SendBlockingRequest(ClientRequest request, double timeout) {
        if (DuelFlow.LoggingEnabled)
            Console.WriteLine("SendBlockingRequest [" + request.RequestId + "]");

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
        object readyLock = new();
        // Create the read event.
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
        var result = blocker.StartBlock(timeout * Settings.BlockPlaybackDurationMultiplier);
        // After the block has finished no longer need to listen to the delegate.
        Player1.InBoundReadDelegate = delegate { };
        Player2.InBoundReadDelegate = delegate { };
        return (result, player1Ready, player2Ready);
    }


}
