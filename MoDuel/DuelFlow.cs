using MoDuel.Tools;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoDuel.Cards;
using MoDuel.Mana;

namespace MoDuel {

    [MoonSharpUserData]
    public partial class DuelFlow {

        /// <summary>
        /// The lua systems and loaded content provided to the <see cref="DuelFlow"/> on it's creation.
        /// </summary>
        private readonly EnvironmentContainer Environment;

        /// <summary>
        /// The current state of the duel.
        /// <para>Holds most of the duel variables.</para>
        /// </summary>
        [MoonSharpHidden]
        public DuelState State { get; private set; }

        /// <summary>
        /// The animations that are sent out from the duel flow.
        /// </summary>
        [MoonSharpHidden]
        public EventHandler<ClientRequest> OutBoundDelegate;

        public readonly CardInstanceActivator CardInstanceActivator = new CardInstanceActivator();

        /// <summary>
        /// A repeating <see cref="System.Timers.Timer"/> that is used to determine when a turn should be considered timed out.
        /// <para>Pause with <see cref="System.Timers.Timer.Stop"/> and Resume with <see cref="System.Timers.Timer.Start"/></para>
        /// <para>The timer should be paused during certain actions (e.g. Animations) To allow a player equal time.</para>
        /// </summary>
        private readonly System.Timers.Timer TimeOutTimer;

        /// <summary>
        /// Resumes the loop if the thread is stuck waiting.
        /// <para>For the loop to be stuck waiting the <see cref="DuelState.CommandQueue"/> will be empty.</para>
        /// <para>Ensures that the loop thread is only waiting when neccesary.</para>
        /// </summary>
        private readonly ManualResetEvent ContinueEvent = new ManualResetEvent(false);

        /// <summary>
        /// Object to ensure that multiple object don't use the <see cref="Script"/> at the same time.
        /// </summary>
        private readonly object ThreadLock = new object();

        [MoonSharpHidden]
        public DuelFlow(EnvironmentContainer environment, Player player1, Player player2) {
            if (environment.Settings.TimeOutPlayers)
                TimeOutTimer = new System.Timers.Timer(environment.Settings.TimeOutInterval);

            Environment = environment;

            // Get the player that is going first if one is forced to.
            Player goesFirst;
            if (environment.Settings.ForceIdToGoFirst == player1.UserId)
                goesFirst = player1;
            else if (environment.Settings.ForceIdToGoFirst == player2.UserId)
                goesFirst = player2;
            else
                // If neither player is explictly going first than we randomly selected one them using the seeded random.
                goesFirst = environment.Random.NextItem(new Player[] { player1, player2 });

            State = new DuelState(player1, player2) {
                CurrentTurn = new TurnData(goesFirst)
            };

            SetupLua();
        }

        /// <summary>
        /// Starts the <see cref="Loop"/> on a new thread.
        /// <para>Will also start the cooldown for timing out players.</para>
        /// </summary>
        /// <returns>The thread the loop started on.</returns>
        [MoonSharpHidden]
        public Thread StartLoop() {

            if (State == null || !State.Unfinished)
                return null;

            if (Environment.Settings.TimeOutPlayers) {
                TimeOutTimer.Elapsed += TimeOutTimer_Elapsed;
                TimeOutTimer.Start();
            }
            Thread thread = new Thread(new ThreadStart(Loop));
            thread.Start();
            return thread;
        }


        /// <summary>
        /// Action invoked when <see cref="TimeOutTimer"/> reaches its interval. 
        /// <para>Calls <see cref="DuelSettings.TimeOutAction"/> on the current turn owner.</para>
        /// </summary>
        private void TimeOutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            lock (ThreadLock) {
                // TODO: Clear Commands
                Environment.Lua.AsScript.Call(Environment.Settings.TimeOutAction, State.CurrentTurn.Owner);
            }
        }

        /// <summary>
        /// Starts the timer again.
        /// </summary>
        public void ResetTimer() {
            if (Environment.Settings.TimeOutPlayers)
                TimeOutTimer.Start();
        }

        [MoonSharpHidden]
        public void Loop() {

            //Call the game start action. This is a user defined function that sets up anything that needs to be setup in lua.
            Environment.Settings.GameStartAction.Call();

            while (State.Unfinished) {
                //If there is anything in the command queue we try it.
                if (CommandBuffer.Count > 0) {
                    //Stop timing out players in case commands take a long time.
                    if (Environment.Settings.TimeOutPlayers)
                        TimeOutTimer.Stop();
                    // We cant confirm that commands is empty at this point due to multithreading.
                    try {
                        KeyValuePair<CommandRefrence, Action>? command = null;
                        // We lock for removal and retreival.
                        lock (CommandBuffer) {
                            command = CommandBuffer.FirstOrDefault();
                            if (command != null) {
                                // Ensure the command is still in the command list it could have be replaced with a new command but we dont care about it.
                                if (CommandBuffer.ContainsKey(command.Value.Key))
                                    CommandBuffer.Remove(command.Value.Key);
                            }
                        }
                        // Invoke the command. We can't lock the command buffer. here as it would ruin the buffer.
                        command?.Value?.Invoke();
                    }
                    catch (Exception e) {
                        // Resulting code of the command could be broken or the command buffer could have been emptied.
                        Console.Write(e.StackTrace);            
                    } 
                    ResetTimer();
                    ContinueEvent.Reset();
                    continue;
                }
                //If the command queue is empty we wait for it to be propegated.
                ContinueEvent.WaitOne();
            }

            // Call the cleanup function provided in settings.
            Environment.Settings.GameCleanUp?.Invoke();

        }


        /// <summary>
        /// Checks and returns any reactions to the given <paramref name="trigger"/>.
        /// <para>Checks their hero abiltites, grave triggers, hand triggers and alive triggers in order.</para>
        /// <para>Each type of reaction happens for both players before going to the next with the turn players first.</para>
        /// </summary>
        /// <param name="trigger">The trigger keyword.</param>
        /// <returns>A list of pairs with the keys being the sender and the values are the reaction functions that should be run in sequence.</returns> 
        [MoonSharpHidden]
        private KeyValuePair<object, Closure>[] FindReactions(string trigger) {

            List<KeyValuePair<object, Closure>> Reactions = new List<KeyValuePair<object, Closure>>();
            //Get the two players.
            var owner = State.CurrentTurn.Owner;
            var opposer = State.GetOpposingPlayer(owner);

            //Set reaction to null so we can you boolean shorthand.
            Closure reaction = null;

            //Check to see if the heroes should activate any abilities.
            if (owner.Hero.Imprint.TriggerReactions?.TryGetValue(trigger, out reaction) ?? false)
                Reactions.Add(new KeyValuePair<object, Closure>(owner.Hero, reaction));
            if (opposer.Hero.Imprint.TriggerReactions?.TryGetValue(trigger, out reaction) ?? false)
                Reactions.Add(new KeyValuePair<object, Closure>(opposer.Hero, reaction));


            //Get the triggerreaction of each card.
            foreach (var card in CardInstanceActivator.EnabledCIs) {
                if (card.Imprint.TriggerReactions?.TryGetValue(trigger, out reaction) ?? false)
                    Reactions.Add(new KeyValuePair<object, Closure>(card, reaction));
            }

            //Iterate over ongoing effects in the order they were created.
            foreach (var eff in OngoingEffect.OngoingEffects) {
                if (eff.TryGetReaction(trigger, out reaction))
                    Reactions.Add(new KeyValuePair<object, Closure>(eff, reaction));
            }

            return Reactions.ToArray();
        }

    }
}
