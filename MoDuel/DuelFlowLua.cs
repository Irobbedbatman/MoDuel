using MoDuel.Animation;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Field;
using MoDuel.Heroes;
using MoonSharp.Interpreter;
using System;
using System.Linq;
using MoonSharp.Environment;
using MoDuel.Tools;

namespace MoDuel {

    public partial class DuelFlow {

        private void SetupLua() {
            UserData.RegisterAssembly();
            JArrayProxy.Register();
            Environment.Lua.AsScript.Globals["State"] = State;
            Environment.Lua.AsScript.Globals["Flow"] = this;
            Environment.Lua.AsScript.Globals["Random"] = Environment.Random;
            Environment.Lua.AsScript.Globals["GetTarget"] = (Func<int, Target>)Target.GetTarget;
            Environment.Lua.AsScript.Globals["CPResponse"] = (Func<string[], CanPlayResponse>)CanPlayResponse.New;
            Environment.Lua.AsScript.Globals["CanPlayTest"] = new CanPlayResponse("Test");
            Environment.Lua.AsScript.Globals["PrintTable"] = (Func<Table, string>)(
                (t) => string.Join("\n", t.Pairs.Select(
                    (p) => p.Key.ToString() + ": " + p.Value.ToPrintString()
                    )
                ));
        }

        #region Constructor Lua Accessors 
        public Card GetCard(string cardId) => Environment.Content.GetCard(cardId);
        public Hero GetHero(string heroId) => Environment.Content.GetHero(heroId);

        public CardInstance CreateCardInstance(Card imprint) =>  new CardInstance(imprint, CardInstanceActivator);
        public CardInstance CreateCardInstance(Card imprint, Player owner) => new CardInstance(imprint, CardInstanceActivator, owner);

        public CreatureInstance CreateCreature(Card imprint, FieldSlot position) => new CreatureInstance(imprint, CardInstanceActivator, position);
        public CreatureInstance CreateCreature(CardInstance card, FieldSlot position) => new CreatureInstance(card, CardInstanceActivator, position);
        public CreatureInstance CreateCreature(Card imprint, CreatureInstance previousState) => new CreatureInstance(imprint, CardInstanceActivator, previousState);

        public HeroInstance CreateHeroInstance(Hero imprint) => new HeroInstance(imprint);
        public OngoingEffect CreateOngoingEffect() => new OngoingEffect();

        public void NewTurn(Player turnOwner) { State.CurrentTurn = new TurnData(turnOwner); }

        #endregion

        public void GameOver(Player winner) {
            State.OnGoing = false;
            if (Environment.Settings.TimeOutPlayers)
                TimeOutTimer.Stop();
            Environment.Settings.GameEndAction.Function.Call(winner);
        }

        public void ChangeTurns() => Environment.Settings.ChangeTurnAction.Function.Call();

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

            //Ensure we don't go above the maximum trigger amount.
            if (currentTriggerChain >= MAX_TRIGGER_CHAIN)
                return;
            currentTriggerChain++;
            foreach (var reaction in FindReactions(trigger)) {
                Environment.Lua.AsScript.Call(reaction.Value, arguments.Prepend(reaction.Key).ToArray());
            }
            currentTriggerChain--;
        }

        /// <summary>
        /// Triggers reactions found in <see cref="FindReactions(string)"/>
        /// <para>Inciting a trigger rather than using <see cref="Trigger(string, object[])"/> forces you to parse an <paramref name="inciter"/>.</para>
        /// </summary>
        public void InciteTrigger(object inciter, string trigger, params object[] arguments) {
            //Ensure we don't go above the maximum trigger amount.
            if (currentTriggerChain >= MAX_TRIGGER_CHAIN)
                return;
            currentTriggerChain++;
            foreach (var reaction in FindReactions(trigger)) {
                Environment.Lua.AsScript.Call(reaction.Value, arguments.Prepend(reaction.Key).Prepend(inciter).ToArray());
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
                Environment.Lua.AsScript.Call(reaction.Value, reaction.Key, values);
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

            if (triggerer.Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out Closure reaction)) {
                return Environment.Lua.AsScript.Call(reaction, arguments.Prepend(triggerer).ToArray());
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
            if (triggerer.Imprint.ExplicitTriggerReactions.TryGetValue(trigger, out Closure reaction)) {
                return Environment.Lua.AsScript.Call(reaction, arguments.Prepend(triggerer).ToArray());
            }
            else {
                return DoAction(fallbackAction, arguments.Prepend(triggerer).ToArray());
            }
        }

        /// <summary>
        /// The contoller of animation blocking.
        /// </summary>
        private readonly AnimationBlockingHandler _animationBlocker = new AnimationBlockingHandler();

        /// <summary>
        /// Invokes <see cref="OutBoundDelegate"/> with the animation data.
        /// <para>Blocks the thread for <paramref name="blockTime"/> if <see cref="DuelSettings.AnimationSpeed"/> is not <see cref="DuelSettings.NO_ANIM"/>.</para>
        /// </summary>
        /// <param name="animationId">The animation to play.</param>
        /// <param name="blockTime">How long to block the thread by, affected by <see cref="DuelSettings.AnimationSpeed"/>/</param>
        /// <param name="arguments">Arguments sent outwards for the animation.</param>
        public void PlayAnimation(string animationId, double blockTime, params string[] arguments) {
            OutBoundDelegate?.Invoke(this, new AnimationData(animationId, arguments));
            if (Environment.Settings.AnimationSpeed != DuelSettings.NO_ANIM) {
                _animationBlocker.PlayAnimationBlock(blockTime / Environment.Settings.AnimationSpeed);
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
            target.SendAnimation(new AnimationData(animationId, arguments));
            if (Environment.Settings.AnimationSpeed != DuelSettings.NO_ANIM) {
                _animationBlocker.PlayAnimationBlock(blockTime / Environment.Settings.AnimationSpeed);
            }
        }

        /// <summary>
        /// Calls a function stored in the lua enviornment with the provided name and arguments.
        /// <para>Returns null if <see cref="DuelState.OnGoing"/> is false.</para>
        /// </summary>
        /// <param name="actionId">The action's name stored in <see cref="ContentLoader"/> That was loaded with <see cref="ContentLoader.LoadAction(Script, string)"/></param>
        public DynValue DoAction(string actionId, params object[] arguments) {
            if (State.OnGoing)
                if (Environment.Content.TryGetAction(actionId, out Closure func))
                 return Environment.Lua.AsScript.Call(func, arguments);
            return null;
        }

        /// <summary>
        /// Calls a function stored in the lua enviornment with the provided name and arguments.
        /// <para>Same as <see cref="DoAction(string, object[])"/> but forces you provide a sender argument.</para>
        /// </summary>
        public DynValue InciteAction(object inciter, string actionId, params object[] arguments) {
            if (State.OnGoing)
                if (Environment.Content.TryGetAction(actionId, out Closure func))
                    return Environment.Lua.AsScript.Call(func, arguments.Prepend(inciter).ToArray());
            return null;
        }

    }
}
