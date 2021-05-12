using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Cards {



    /// <summary>
    /// <see cref="CardInstanceActivator"/> tells the <see cref="DuelFlow.FindReactions(string)"/> what cards are currently enabled for triggering.
    /// </summary>
    [MoonSharpUserData]
    public class CardInstanceActivator {

        /// <summary>
        /// The list of all enabled <see cref="CardInstance"/> for triggering effects.
        /// </summary>
        private readonly List<CardInstance> _enabledCIs = new List<CardInstance>();
        public IReadOnlyList<CardInstance> EnabledCIs => _enabledCIs.AsReadOnly();

        public void Enable(CardInstance ci) {
            _enabledCIs.Add(ci);
        }

        public void Disable(CardInstance ci) {
            // TODO: Automatic disabling of cards.
            _enabledCIs.Remove(ci);
        }

        public bool IsEnabled(CardInstance ci) {
            return _enabledCIs.Contains(ci);
        }

    }
}
