using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel {

    /// <summary>
    /// An effect that is made to persist after a card can no longer handle the triggers.
    /// </summary>
    [MoonSharpUserData]
    public class OngoingEffect : Target {

        /// <summary>
        /// The ist of currently active ongoing effects.
        /// </summary>
        private static readonly List<OngoingEffect> _ongoingEffects = new List<OngoingEffect>();
        /// <summary>
        /// Accessor for the list of on going effects in a readonly way.
        /// </summary>
        public static IReadOnlyList<OngoingEffect> OngoingEffects => _ongoingEffects.AsReadOnly();

        /// <summary>
        /// All the triggers on a this <see cref="OngoingEffect"/>.
        /// </summary>
        private readonly Dictionary<string, Closure> TriggerReactions = new Dictionary<string, Closure>();

        public OngoingEffect() {
            _ongoingEffects.Add(this);
        }

        public void Destroy() {
            _ongoingEffects.Remove(this);
        }

        /// <summary>
        /// Add a trigger reaction to this <see cref="OngoingEffect"/>.
        /// </summary>
        public void AddTrigger(string triggerKey, Closure triggerReaction) => TriggerReactions.Add(triggerKey, triggerReaction);

        /// <summary>
        /// Attermpt to get a trigger reaction from this <see cref="OngoingEffect"/>.
        /// </summary>
        public bool TryGetReaction(string trigger, out Closure value) => TriggerReactions.TryGetValue(trigger, out value);


    }
}
