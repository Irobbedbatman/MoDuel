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

        private Dictionary<string, DynValue> TriggerReactions = new Dictionary<string, DynValue>();

        public OngoingEffect() {
            _ongoingEffects.Add(this);
        }

        public void Destroy() {
            _ongoingEffects.Remove(this);
        }

        public void AddTrigger(string triggerKey, DynValue triggerReaction) => TriggerReactions.Add(triggerKey, triggerReaction);
        public bool TryGetReaction(string trigger, out DynValue value) => TriggerReactions.TryGetValue(trigger, out value);


    }
}
