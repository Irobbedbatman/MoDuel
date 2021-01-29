using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace MoDuel.Heroes {

    [MoonSharpUserData]
    public class Hero {

        /// <summary>
        /// The id of the hero, that is, a unique name for each hero.
        /// </summary>
        public readonly string HeroId;

        /// <summary>
        /// A dictionary with triggers being used as keys and reactions as values.
        /// <para>The <see cref="string"/> is a trigger keyword.</para>
        /// <para>The <see cref="Closure"/> reaction is a lua function.</para>
        /// </summary>
        public readonly IReadOnlyDictionary<string, Closure> TriggerReactions;

        /// <summary>
        /// A dictionary to use miscellaneous values.
        /// </summary>
        public readonly IReadOnlyDictionary<string, DynValue> Parameters;

        public Hero(string heroId, Dictionary<string, Closure> triggerReactions, Dictionary<string, DynValue> parameters) {
            HeroId = heroId;
            TriggerReactions = triggerReactions ?? new Dictionary<string, Closure>();
            Parameters = parameters ?? new Dictionary<string, DynValue>();
        }
        


    }
}
