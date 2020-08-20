using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Mana {


    public class ManaEnumerator : IEnumerator<Mana> {

        public readonly Mana[] _mana;

        private int position = -1;

        public ManaEnumerator(Mana[] mana) {
            _mana = mana;
        }

        public bool MoveNext() {
            position++;
            return position < _mana.Length;
        }

        public void Reset() {
            position = -1;
        }

        public void Dispose() { }

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

        object IEnumerator.Current => Current;

    }
}
