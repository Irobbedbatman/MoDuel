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

        private static readonly List<OngoingEffect> _ongoingEffects = new List<OngoingEffect>();
        public static IReadOnlyList<OngoingEffect> OngoingEffects => _ongoingEffects.AsReadOnly();

        private readonly Dictionary<string, Closure> TriggerReactions = new Dictionary<string, Closure>();

        public OngoingEffect() {
            _ongoingEffects.Add(this);
        }

        public void Destroy() {
            _ongoingEffects.Remove(this);
        }

        public void AddTrigger(string triggerKey, Closure triggerReaction) => TriggerReactions.Add(triggerKey, triggerReaction);
        public bool TryGetReaction(string trigger, out Closure value) => TriggerReactions.TryGetValue(trigger, out value);


    }
}
