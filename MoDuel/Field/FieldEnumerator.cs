using System;
using System.Collections;
using System.Collections.Generic;

namespace MoDuel.Field {

    /// <summary>
    /// The enumerator for iterating over an amount of field slots.
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class FieldEnumerator : IEnumerator<FieldSlot> {

        /// <summary>
        /// The slots to enumerate through,
        /// </summary>
        private readonly FieldSlot[] _slots;

        /// <summary>
        /// The poition in <see cref="_slots"/> we are looking at.
        /// </summary>
        private int position = -1;

        public FieldEnumerator(FieldSlot[] slots) {
            _slots = slots;
        }

        /// <inheritdoc/>
        public bool MoveNext() {
            position++;
            return position < _slots.Length;
        }

        /// <inheritdoc/>
        public void Reset() {
            position = -1;
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        object IEnumerator.Current => Current;
    }
}
