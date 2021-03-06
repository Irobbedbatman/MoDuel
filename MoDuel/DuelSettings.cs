using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel {

    [MoonSharpUserData]
    public class DuelSettings {

        public const float NO_ANIM = 0;

        public const double DEFAULT_TIMEOUT = 5 * 60 * 1000;

        /// <summary>
        /// Makes the <see cref="Player"/> with the same ID to go first when <see cref="DuelFlow.InitState(Player, Player)"/> is called.
        /// </summary>
        public string ForceIdToGoFirst = "";

        /// <summary>
        /// The speed animations will played back at. defaults to no animations.
        /// <para>Set to <see cref="NO_ANIM"/> to disable animations.</para>
        /// </summary>
        public float AnimationSpeed = NO_ANIM;

        /// <summary>
        /// Shorthand check to see if animations should play.
        /// </summary>
        public bool PlayAnimations => AnimationSpeed != NO_ANIM;

        public bool TimeOutPlayers = false;

        public double TimeOutInterval = DEFAULT_TIMEOUT;

        /// <summary>
        /// The <see cref="DynValue"/> function that is used when a player takes to long on their turn when <see cref="TimeOutPlayers"/> is true. 
        /// </summary>
        public Closure TimeOutAction;

        /// <summary>
        /// Action that is called when the game starts.
        /// </summary>
        public Closure GameStartAction;

        /// <summary>
        /// Action that is called when the game ends.
        /// </summary>
        public Closure GameEndAction;

        /// <summary>
        /// Action to resolve once the game is over.
        /// </summary>
        public Action GameCleanUp;

        /// <summary>
        /// Action that is called each time the turn is changed.
        /// </summary>
        public Closure ChangeTurnAction;

    }
}
