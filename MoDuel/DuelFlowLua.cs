using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Field;
using MoDuel.Heroes;
using MoonSharp.Interpreter;
using System;
using System.Linq;
using MoDuel.Tools;
using Newtonsoft.Json.Linq;
using MoDuel.Mana;

namespace MoDuel {

    public partial class DuelFlow {

        private void SetupLua() {
            UserData.RegisterAssembly();
            UserData.RegisterExtensionType(typeof(Enumerable));
            JArrayProxy.Register();
            JObjectProxy.Register();
            Environment.Lua.AsScript.Globals["Flow"] = this;
            Environment.Lua.AsScript.Globals["State"] = State;
            Environment.Lua.AsScript.Globals["Random"] = Environment.Random;
            Environment.Lua.AsScript.Globals["GetTarget"] = (Func<int, Target>)Target.GetTarget;
            Environment.Lua.AsScript.Globals["CPResponse"] = (Func<CanPlayResponse>)CanPlayResponse.New;
            Environment.Lua.AsScript.Globals["CanPlayTest"] = new CanPlayResponse("Test");
            Environment.Lua.AsScript.Globals["PrintTable"] = (Func<Table, string>)(
                (t) => string.Join("\n", t.Pairs.Select(
                    (p) => p.Key.ToString() + ": " + p.Value.ToPrintString()
                    )
                ));
            Environment.Lua.AsScript.Globals["IsNumber"] = (Func<DynValue, bool>)(
                (value) => ValueIsType(value, DataType.Number));
            Environment.Lua.AsScript.Globals["IsString"] = (Func<DynValue, bool>)(
                (value) => ValueIsType(value, DataType.String));
            Environment.Lua.AsScript.Globals["IsBool"] = (Func<DynValue, bool>)(
                (value) => ValueIsType(value, DataType.Boolean));
            Environment.Lua.AsScript.Globals["IsTable"] = (Func<DynValue, bool>)(
                (value) => ValueIsType(value, DataType.Table));
            Environment.Lua.AsScript.Globals["IsObject"] = (Func<DynValue, bool>)(
                (value) => ValueIsType(value, DataType.UserData));
            Environment.Lua.AsScript.Globals["IsFunc"] = (Func<DynValue, bool>)(
                (value) => ValueIsType(value, DataType.Function));
        }

        #region Constructor Lua Accessors 
        public Card GetCard(string cardId) => Environment.Content.GetCard(cardId);
        public Hero GetHero(string heroId) => Environment.Content.GetHero(heroId);
        public Closure GetAction(string actionId) => Environment.Content.GetAction(actionId);
        public JObject GetJsonFile(string filename) => Environment.Content.GetFile(filename);
        public CardInstance CreateCardInstance(Card imprint) =>  new CardInstance(imprint, CardInstanceActivator);
        public CardInstance CreateCardInstance(Card imprint, Player owner) => new CardInstance(imprint, CardInstanceActivator, owner);
        public ManaType GetMana(string name) => new ManaType(name);
        public CreatureInstance CreateCreature(Card imprint, FieldSlot position) => new CreatureInstance(imprint, CardInstanceActivator, position);
        public CreatureInstance CreateCreature(CardInstance card, FieldSlot position) => new CreatureInstance(card, CardInstanceActivator, position);
        public CreatureInstance CreateCreature(Card imprint, CreatureInstance previousState) => new CreatureInstance(imprint, CardInstanceActivator, previousState);
        public OngoingEffect CreateOngoingEffect() => new OngoingEffect();

        /// <summary>
        /// Creates a new turn sets as the current turn and returns it.
        /// </summary>
        public TurnData NewTurn(Player turnOwner) { 
            var newTurn = new TurnData(turnOwner);
            State.CurrentTurn = newTurn;
            return newTurn;
        }

        #endregion

        /// <summary>
        /// Call from lua to determine a winner and end the flow.
        /// </summary>
        public void GameOver(Player winner) {
            State.Unfinished = false;
            if (Environment.Settings.TimeOutPlayers)
                TimeOutTimer.Stop();
            Environment.Settings.GameEndAction.Call(winner);
        }

        public void ChangeTurns() => Environment.Settings.ChangeTurnAction.Call();

        /// <summary>
        /// How many triggers have activated sequentially.
        /// </summary>
        private float currentTriggerChain = 1;
        
        /// <summary>
        /// How many triggers can be activated depth wise. Used to dodge most missable infinite loops.
        /// <para>Calling the same action within itself is one example of a endless loop unaffected.</para>
        /// </summary>
        public const float MAX_TRIGGER_CHAIN = 22;

        /// <summary>
        /// Triggers reactions found in <see cref="FindReactions(string)"/>
        /// <para>Use <see cref="InciteTrigger(object, string, object[])"/> if you need to provide the reason the trigger occured.</para>
        /// </summary>
        public void Trigger(string trigger, params object[] arguments) {
            //Ensure that the game is ongoing.
            if (!State.Unfinished)
                return;
            //Ensure we don't go above the maximum trigger amount.
            if (currentTriggerChain >= MAX_TRIGGER_CHAIN)
                return;
            currentTriggerChain++;
            foreach (var reaction in FindReactions(trigger)) {
                reaction.Value.Call(arguments.Prepend(reaction.Key).ToArray());
            }
            currentTriggerChain--;
        }

        /// <summary>
        /// Triggers reactions found in <see cref="FindReactions(string)"/>
        /// <para>Inciting a trigger rather than using <see cref="Trigger(string, object[])"/> forces you to parse an <paramref name="inciter"/>.</para>
        /// </summary>
        public void InciteTrigger(object inciter, string trigger, params object[] arguments) {
            //Ensure that the game is ongoing.
            if (!State.Unfinished)
                return;
            //Ensure we don't go above the maximum trigger amount.
            if (currentTriggerChain >= MAX_TRIGGER_CHAIN)
                return;
            currentTriggerChain++;
            foreach (var reaction in FindReactions(trigger)) {
                reaction.Value.Call(arguments.Prepend(reaction.Key).Prepend(inciter).ToArray());
            }
            currentTriggerChain--;
        }

        /// <summary>
        /// A trigger that is called so that outcomes can change for specific actions.
        /// <para>Should not be used to perform game state changes only changes to the outcome of the action that trigger it.</para>
        /// <para>Reactions recieved in <see cref="FindReactions(string)"/> are reversed so that higher priority triggerers have stronger impact.</para>
        /// </summary>
        /// <param name="values">The table that is parsed by refrence so that things can chnage.</param>
        public void OverwriteTrigger(string trigger, Table values) {
            //Ensure that the game is ongoing.
            if (!State.Unfinished)
                return;
            foreach (var reaction in FindReactions(trigger).Reverse()) {
                reaction.Value.Call(reaction.Key, values);
            }
        }

        /// <summary>
        /// Calls the explicit trigger on a card instance.
        /// </summary>
        /// <param name="triggerer">The card instance that we check the trigger on; is also added as first argument to lua function.</param>
        /// <param name="trigger">The trigger key word to check reactions for in <see cref="Card.ExplicitTriggerReactions"/></param>
        /// <param name="arguments">The list of arguments to send to the lua functions. Prepended with <paramref name="triggerer"/>.</param>
        public DynValue ExplicitTrigger(CardInstance triggerer, string trigger, params object[] arguments) {
            //Ensure that the game is ongoing.
            if (!State.Unfinished)
                return null;

            if (triggerer.Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out Closure reaction)) {
                return reaction.Call(arguments.Prepend(triggerer).ToArray());
            }
            return null;
        }

        /// <summary>
        /// Calls the explicit trigger on a card instance.
        /// </summary>
        /// <param name="triggerer">The card instance that we check the trigger on; is also added as first argument to lua function.</param>
        /// <param name="trigger">The trigger key word to check reactions for in <see cref="Card.ExplicitTriggerReactions"/></param>
        /// <param name="arguments">The list of arguments to send to the lua functions. Prepended with <paramref name="triggerer"/>.</param>
        public DynValue FallbackExplicitTrigger(CardInstance triggerer, string trigger, string fallbackAction, params object[] arguments) {
            //Ensure that the game is ongoing.
            if (!State.Unfinished)
                return null;
            if (triggerer.Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out Closure reaction)) {
                return reaction.Call(arguments.Prepend(triggerer).ToArray());
            }
            else {
                return DoAction(fallbackAction, arguments.Prepend(triggerer).ToArray());
            }
        }

        /// <summary>
        /// Invokes <see cref="OutBoundDelegate"/> with a request for the client to do.
        /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the reuqest should stop other things from happening.</para>/// </summary>
        /// <param name="requestId">The request for the client to do.</param>
        /// <param name="arguments">Arguments sent outwards for the animation.</param>
        public void SendRequest(string requestId, params DynValue[] arguments) {
            OutBoundDelegate?.Invoke(this, new ClientRequest(requestId, arguments));
        }

        /// <summary>
        /// Invokes <see cref="Player.OutBoundDelegate"/> with a request for the client to do.
        /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>
        /// /// </summary>
        /// <param name="requestId">The request for the target to do.</param>
        /// <param name="arguments">Arguments sent outwards for the animation.</param>
        public void SendRequestTo(Player target, string requestId, params DynValue[] arguments) {
            target.SendRequest(requestId, arguments);
        }

        /// <summary>
        /// Blocks the execution of anything on the <see cref="DuelFlow"/> <see cref="Thread"/>.
        /// </summary>
        /// <param name="blockDuration">How long to block the thread by, affected by <see cref="DuelSettings.AnimationSpeed"/></param>
        public void BlockPlayback(double blockDuration) {
            if (Environment.Settings.PlayAnimations) {
                Environment.AnimationBlocker.StartBlock(blockDuration / Environment.Settings.AnimationSpeed);
            }
        }

        /// <summary>
        /// Calls a function stored in the lua enviornment with the provided name and arguments.
        /// <para>Returns null if <see cref="DuelState.Unfinished"/> is false.</para>
        /// </summary>
        /// <param name="actionId">The action's name stored in <see cref="ContentLoader"/> That was loaded with <see cref="ContentLoader.LoadAction(Script, string)"/></param>
        public DynValue DoAction(string actionId, params object[] arguments) {
            Console.WriteLine(actionId);
            if (State.Unfinished)
                if (Environment.Content.TryGetAction(actionId, out Closure func))
                    return func.Call(arguments);
            return null;
        }

        /// <summary>
        /// Calls a function stored in the lua enviornment with the provided name and arguments.
        /// <para>Same as <see cref="DoAction(string, object[])"/> but forces you provide a sender argument.</para>
        /// </summary>
        public DynValue InciteAction(object inciter, string actionId, params object[] arguments) {
            if (State.Unfinished)
                if (Environment.Content.TryGetAction(actionId, out Closure func))
                    return func.Call(arguments.Prepend(inciter).ToArray());
            return null;
        }

        /// <summary>
        /// Called when a player wants to check something and get a result outside of game flow purposes.
        /// <para>Needs to lock the thread because it is likely called from a diffrent thread.</para>
        /// <para>The response is sent back trhough the <paramref name="response"/> action.</para>
        /// </summary>
        public void Query(Player querier, string query, Action<Table> response, params object[] args) {
            lock (ThreadLock) {
                response?.Invoke(DoAction(query, args.Prepend(querier).ToArray()).Table);
            }
        }

        /// <summary>
        /// Checks to see if <paramref name="value"/> is the provided <paramref name="type"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns>True if value is the valid type. False otherwise or if <paramref name="value"/> is null.</returns>
        public bool ValueIsType(DynValue value, DataType type) {
            if (value == null)
                return false;
            return value.Type == type;
        }

    }
}
