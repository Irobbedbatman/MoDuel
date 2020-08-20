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
        /// <para>The <see cref="DynValue"/> reaction is a lua function.</para>
        /// </summary>
        public readonly IReadOnlyDictionary<string, DynValue> TriggerReactions;

        /// <summary>
        /// A dictionary to use miscellaneous values.
        /// </summary>
        public readonly IReadOnlyDictionary<string, DynValue> Parameters;

        public Hero(string heroId, Dictionary<string, DynValue> triggerReactions, Dictionary<string, DynValue> parameters) {
            HeroId = heroId;
            TriggerReactions = triggerReactions ?? new Dictionary<string, DynValue>();
            Parameters = parameters ?? new Dictionary<string, DynValue>();
        }
        


    }
}
