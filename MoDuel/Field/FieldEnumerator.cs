using System;
using System.Collections;

namespace MoDuel.Field {

    /// <summary>
    /// The enumerator for iterating over an amount of field slots.
    /// </summary>
    public class FieldEnumerator : IEnumerator {

        public readonly FieldSlot[] _slots;

        private int position = -1;

        public FieldEnumerator(FieldSlot[] slots) {
            _slots = slots;
        }

        public bool MoveNext() {
            position++;
            return position < _slots.Length;
        }

        public void Reset() {
            position = -1;
        }

        public FieldSlot Current {
            get {
                try {
                    return _slots[position];
                }
                catch {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;
    }
}
