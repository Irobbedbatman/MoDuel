using MoDuel.Tools;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoDuel.Cards;
using MoDuel.Animation;
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
        public EventHandler<AnimationData> OutBoundDelegate;


        public CardInstanceActivator CardInstanceActivator = new CardInstanceActivator();

        /// <summary>
        /// The time it takes for a turn to timeout in milliseconds.
        /// </summary>
        public readonly static double TimeOutInterval = 5 * 60 * 1000;
        //TODO: Variable time out intervals.

        /// <summary>
        /// A repeating <see cref="System.Timers.Timer"/> that is used to determine when a turn should be considered timed out.
        /// <para>Pause with <see cref="System.Timers.Timer.Stop"/> and Resume with <see cref="System.Timers.Timer.Start"/></para>
        /// <para>The timer should be paused during certain actions (e.g. Animations) To allow a player equal time.</para>
        /// </summary>
        private System.Timers.Timer TimeOutTimer = new System.Timers.Timer(TimeOutInterval);

        /// <summary>
        /// Resumes the loop if the thread is stuck waiting.
        /// <para>For the loop to be stuck waiting the <see cref="DuelState.CommandQueue"/> will be empty.</para>
        /// <para>Ensures that the loop thread is only waiting when neccesary.</para>
        /// </summary>
        private ManualResetEvent ContinueEvent = new ManualResetEvent(false);

        [MoonSharpHidden]
        public DuelFlow(EnvironmentContainer environment, DuelSettings settings, Player player1, Player player2, Player goesFirst) {
            State = new DuelState(player1, player2, settings) {
                CurrentTurn = new TurnData(goesFirst)
            };
            Environment = environment;
            SetupLua();
        }

        [MoonSharpHidden]
        public Thread Start() {
            if (State.Settings.TimeOutPlayers) {
                TimeOutTimer.Elapsed += TimeOutTimer_Elapsed;
                TimeOutTimer.Start();
            }
            State.OnGoing = true;
            Thread thread = new Thread(new ThreadStart(Loop));
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Action invoked when <see cref="TimeOutTimer"/> reaches its interval. 
        /// <para>Calls <see cref="DuelSettings.TimeOutAction"/> on the current turn owner.</para>
        /// </summary>
        private void TimeOutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            //TODO: Thread safety
            Environment.Lua.AsScript.Call(State.Settings.TimeOutAction, State.CurrentTurn.TurnOwner);
        }

        [MoonSharpHidden]
        public void Loop() {

            //Call the game start action. This is a user defined function that sets up anything that needs to be setup in lua.
            State.Settings.GameStartAction.Function.Call();

            while (State.OnGoing) {
                //If there is anything in the command queue we try it.
                if (CommandQueue.Count > 0) {
                    //Stop timing out players in case commands take a long time.
                    if (State.Settings.TimeOutPlayers)
                        TimeOutTimer.Stop();
                    if (CommandQueue.TryDequeue(out var result))
                        result.Invoke();
                    if (State.Settings.TimeOutPlayers)
                        TimeOutTimer.Start();
                    ContinueEvent.Reset();
                    continue;
                }
                //If the command queue is empty we wait for it to be propegated.
                ContinueEvent.WaitOne();
            }
            GameFinished();
        }

        [MoonSharpHidden]
        public void GameFinished() {
            //TODO: Game Finished
        }

        /// <summary>
        /// Checks and returns any reactions to the given <paramref name="trigger"/>.
        /// <para>Checks their hero abiltites, grave triggers, hand triggers and alive triggers in order.</para>
        /// <para>Each type of reaction happens for both players before going to the next with the turn players first.</para>
        /// </summary>
        /// <param name="trigger">The trigger keyword.</param>
        /// <returns>A list of pairs with the keys being the sender and the values are the reaction functions that should be run in sequence.</returns> 
        [MoonSharpHidden]
        private KeyValuePair<object, DynValue>[] FindReactions(string trigger) {

            //TODO: Bake Reactions

            List<KeyValuePair<object, DynValue>> Reactions = new List<KeyValuePair<object, DynValue>>();
            //Get the two players.
            var owner = State.CurrentTurn.TurnOwner;
            var opposer = State.GetOpposingPlayer(owner);

            //Set reaction to null so we can you boolean shorthand.
            DynValue reaction = null;

            //Check to see if the heroes should activate any abilities.
            if (owner.CurrentHero.Imprint.TriggerReactions?.TryGetValue(trigger, out reaction) ?? false)
                Reactions.Add(new KeyValuePair<object, DynValue>(owner.CurrentHero, reaction));
            if (opposer.CurrentHero.Imprint.TriggerReactions?.TryGetValue(trigger, out reaction) ?? false)
                Reactions.Add(new KeyValuePair<object, DynValue>(opposer.CurrentHero, reaction));


            //Get the triggerreaction of each card.
            foreach (var card in CardInstanceActivator.EnabledCIs) {
                if (card.Imprint.TriggerReactions?.TryGetValue(trigger, out reaction) ?? false)
                    Reactions.Add(new KeyValuePair<object, DynValue>(card, reaction));
            }

            //Iterate over ongoing effects in the order they were created.
            foreach (var eff in OngoingEffect.OngoingEffects) {
                if (eff.TryGetReaction(trigger, out reaction))
                    Reactions.Add(new KeyValuePair<object, DynValue>(eff, reaction));
            }

            return Reactions.ToArray();
        }

    }
}
