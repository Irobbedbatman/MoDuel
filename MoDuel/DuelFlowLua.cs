using MoDuel.Animation;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Field;
using MoDuel.Heroes;
using MoonSharp.Interpreter;
using System;
using System.Linq;

namespace MoDuel {

    public partial class DuelFlow {

        /// <summary>
        /// Accessor for the singleton <see cref="Tools.LuaEnvironment"/>.
        /// </summary>
        [MoonSharpHidden]
        private Script LuaEnvironment => Tools.LuaEnvironment.Instance;

        /// <summary>
        /// Accessor for the singleton <see cref="Tools.LuaEnvironment"/>.
        /// </summary>
        [MoonSharpHidden]
        private LoadedContent Content => LoadedContent.Instance;

        private void SetupLua() {
            UserData.RegisterAssembly();
            LuaEnvironment.Globals["State"] = State;
            LuaEnvironment.Globals["Flow"] = this;
            LuaEnvironment.Globals["SharpRandom"] = new Tools.LuaExtensions.SharpRandom();
            LuaEnvironment.Globals["GetTarget"] = (Func<int, Target>)Target.GetTarget;
            LuaEnvironment.Globals["CPResponse"] = (Func<string[], CanPlayResponse>)CanPlayResponse.New;
            LuaEnvironment.Globals["CanPlayTest"] = new CanPlayResponse("Test");
            LuaEnvironment.Globals["PrintTable"] = (Func<Table, string>)(
                (t) => string.Join("\n", t.Pairs.Select(
                    (p) => p.Key.ToString() + ": " + p.Value.ToPrintString()
                    )
                ));
        }

        #region Constructor Lua Accessors 

        public Card GetCard(string cardId) => Content.GetCard(cardId);
        public Hero GetHero(string heroId) => Content.GetHero(heroId);

        public CardInstance CreateCardInstance(Card imprint) =>  new CardInstance(imprint, CardInstanceActivator);
        public CardInstance CreateCardInstance(Card imprint, Player owner) => new CardInstance(imprint, CardInstanceActivator, owner);

        public CreatureInstance CreateCreature(Card imprint, FieldSlot position) => new CreatureInstance(imprint, CardInstanceActivator, position);
        public CreatureInstance CreateCreature(CardInstance card, FieldSlot position) => new CreatureInstance(card, CardInstanceActivator, position);
        public CreatureInstance CreateCreature(Card imprint, CreatureInstance previousState) => new CreatureInstance(imprint, CardInstanceActivator, previousState);

        public HeroInstance CreateHeroInstance(Hero imprint) => new HeroInstance(imprint);
        public OngoingEffect CreateOngoingEffect() => new OngoingEffect();

        public void NewTurn(Player turnOwner) { State.CurrentTurn = new TurnData(turnOwner); }

        /// <summary>
        /// Gets a global from the <see cref="LuaEnvironment"/>.
        /// <para>Can be used to get actions as they are stored globally.</para>
        /// </summary>
        public static DynValue GetGlobal(string varName) => Tools.LuaEnvironment.Instance.Globals.Get(varName);

        #endregion

        public void GameOver(Player winner) {
            State.OnGoing = false;
            if (State.Settings.TimeOutPlayers)
                TimeOutTimer.Stop();
            State.Settings.GameEndAction.Function.Call(winner);
        }

        public void ChangeTurns() => State.Settings.ChangeTurnAction.Function.Call();

        public void ResetTimer() {
            if (State.Settings.TimeOutPlayers)
                TimeOutTimer.Start();
        }

        /// <summary>
        /// How many triggers have activated sequentially.
        /// </summary>
        private float currentTriggerChain = 1;
        
        /// <summary>
        /// How many triggers can be activated depth wise. Used to dodge most missable infinite loops.
        /// <para>Calling the same action within itself is one example of a endless loop unaffected.</para>
        /// </summary>
        public static readonly float MaxTriggerChain = 22;

        /// <summary>
        /// Triggers reactions found in <see cref="FindReactions(string)"/>
        /// <para>Use <see cref="InciteTrigger(object, string, object[])"/> if you need to provide the reason the trigger occured.</para>
        /// </summary>
        public void Trigger(string trigger, params object[] arguments) {

            //Ensure we don't go above the maximum trigger amount.
            if (currentTriggerChain >= MaxTriggerChain)
                return;
            currentTriggerChain++;
            foreach (var reaction in FindReactions(trigger)) {
                LuaEnvironment.Call(reaction.Value, arguments.Prepend(reaction.Key).ToArray());
            }
            currentTriggerChain--;
        }

        /// <summary>
        /// Triggers reactions found in <see cref="FindReactions(string)"/>
        /// <para>Inciting a trigger rather than using <see cref="Trigger(string, object[])"/> forces you to parse an <paramref name="inciter"/>.</para>
        /// </summary>
        public void InciteTrigger(object inciter, string trigger, params object[] arguments) {
            //Ensure we don't go above the maximum trigger amount.
            if (currentTriggerChain >= MaxTriggerChain)
                return;
            currentTriggerChain++;
            foreach (var reaction in FindReactions(trigger)) {
                LuaEnvironment.Call(reaction.Value, arguments.Prepend(reaction.Key).Prepend(inciter).ToArray());
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
            if (!State.OnGoing)
                return;

            foreach (var reaction in FindReactions(trigger).Reverse()) {
                LuaEnvironment.Call(reaction.Value, reaction.Key, values);
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
            if (!State.OnGoing)
                return null;

            if (triggerer.Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out DynValue reaction)) {
                return LuaEnvironment.Call(reaction, arguments.Prepend(triggerer).ToArray());
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
            if (!State.OnGoing)
                return null;
            if (triggerer.Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out DynValue reaction)) {
                return LuaEnvironment.Call(reaction, arguments.Prepend(triggerer).ToArray());
            }
            else {
                return DoAction(fallbackAction, arguments.Prepend(triggerer).ToArray());
            }
        }

        /// <summary>
        /// The contoller of animation blocking.
        /// </summary>
        private AnimationBlockingHandler _animationBlocker = new AnimationBlockingHandler();

        /// <summary>
        /// Invokes <see cref="OutBoundDelegate"/> with the animation data.
        /// <para>Blocks the thread for <paramref name="blockTime"/> if <see cref="DuelSettings.AnimationSpeed"/> is not <see cref="DuelSettings.NO_ANIM"/>.</para>
        /// </summary>
        /// <param name="animationId">The animation to play.</param>
        /// <param name="blockTime">How long to block the thread by, affected by <see cref="DuelSettings.AnimationSpeed"/>/</param>
        /// <param name="arguments">Arguments sent outwards for the animation.</param>
        public void PlayAnimation(string animationId, double blockTime, params string[] arguments) {
            OutBoundDelegate?.Invoke(new AnimationData(animationId, arguments));
            if (State.Settings.AnimationSpeed != DuelSettings.NO_ANIM) {
                _animationBlocker.PlayAnimationBlock(blockTime / State.Settings.AnimationSpeed);
            }
        }

        /// <summary>
        /// Invokes <see cref="Player.OutBoundDelegate"/> with the animation data so that individual players can recieve animations.
        /// <para>Blocks the thread for <paramref name="blockTime"/> if <see cref="DuelSettings.AnimationSpeed"/> is not <see cref="DuelSettings.NO_ANIM"/>.</para>
        /// </summary>
        /// <param name="animationId">The animation to play.</param>
        /// <param name="blockTime">How long to block the thread by, affected by <see cref="DuelSettings.AnimationSpeed"/>/</param>
        /// <param name="arguments">Arguments sent outwards for the animation.</param>
        public void PlayTargetedAnimation(Player target, string animationId, double blockTime, params string[] arguments) {
            target.InvokeOutBound(new AnimationData(animationId, arguments));
            if (State.Settings.AnimationSpeed != DuelSettings.NO_ANIM) {
                _animationBlocker.PlayAnimationBlock(blockTime / State.Settings.AnimationSpeed);
            }
        }


        /// <summary>
        /// Logs actions and their arguments to <see cref="LuaEnvironment.Output"/>
        /// </summary>
        public bool LogActions = false;

        /// <summary>
        /// Calls a function stored in the lua enviornment with the provided name and arguments.
        /// <para>Returns null if <see cref="DuelState.OnGoing"/> is false.</para>
        /// </summary>
        /// <param name="actionId">The literal global name stored in <see cref="Script.Globals"/>. That was loaded with <see cref="ContentLoader.LoadAction(Script, string)"/></param>
        public DynValue DoAction(string actionId, params object[] arguments) {
            if (State.OnGoing) {
                if (LogActions)
                    Tools.LuaEnvironment.Output?.Invoke("DoAction(" + actionId + ") args: " + string.Join(" ", arguments.Select( (a) => { return a.ToString(); })));
                return LuaEnvironment.Globals.Get(actionId).Function.Call(arguments);
            }
            return null;
        }

        /// <summary>
        /// Calls a function stored in the lua enviornment with the provided name and arguments.
        /// <para>Same as <see cref="DoAction(string, object[])"/> but forces you provide context.</para>
        /// </summary>
        public DynValue InciteAction(object inciter, string actionId, params object[] arguments) {
            if (State.OnGoing) {
                if (LogActions)
                    Tools.LuaEnvironment.Output?.Invoke("InciteAction(" + actionId + ") inciter: " + inciter.ToString() + "| args: " + string.Join(" ", arguments.Select((a) => { return a.ToString(); })));
                return LuaEnvironment.Globals.Get(actionId).Function.Call(arguments.Prepend(inciter).ToArray());
            }
            return null;
        }

    }
}
