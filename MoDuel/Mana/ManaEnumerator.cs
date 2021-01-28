using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Mana {

    /// <summary>
    /// An enumarator used in <see cref="ManaPool"/> to enumarate through it's <see cref="Mana"/> components.
    /// </summary>
    public class ManaEnumerator : IEnumerator<Mana> {

        /// <summary>
        /// The <see cref="Mana"/> array provided in the constructor to enumerate through.
        /// </summary>
        public readonly Mana[] _mana;

        /// <summary>
        /// Position of the <see cref="Current"/> element in <see cref="_mana"/>
        /// </summary>
        private int position = -1;

        public ManaEnumerator(Mana[] mana) {
            _mana = mana;
        }

        /// <inheritdoc/>
        public bool MoveNext() {
            position++;
            return position < _mana.Length;
        }

        /// <inheritdoc/>
        public void Reset() {
            position = -1;
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public Mana Current {
            get {
                try {
                    return _mana[position];
                }
                catch {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <inheritdoc/>
        object IEnumerator.Current => Current;

    }
}
