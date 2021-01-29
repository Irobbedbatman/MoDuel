using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Cards;
using MoonSharp.Interpreter;

namespace MoDuel.Field {

    /// <summary>
    /// A <see cref="Field"/> that stores <seealso cref="CreatureInstance"/>s inside an array <seealso cref="Slots"/>
    /// </summary>
    [MoonSharpUserData]
    public class SubField : Field {

        /// <summary>
        /// The <see cref="Player"/> that this <see cref="SubField"/> is owned by.
        /// </summary>
        public readonly Player Owner;

        /// <summary>
        /// The slots in this field; each a <see cref="FieldSlot"/>.
        /// </summary>
        public readonly FieldSlot[] Slots = new FieldSlot[5];

        public SubField(Player owner) {
            Slots = new FieldSlot[5] { new FieldSlot(this), new FieldSlot(this), new FieldSlot(this), new FieldSlot(this), new FieldSlot(this) };
            Owner = owner;
        }

        /// <inheritdoc/>
        public override FieldSlot this[int index] => Slots[index];

        /// <inheritdoc/>
        public override HashSet<CreatureInstance> GetCreatures() {
            var keys = new HashSet<CreatureInstance>();
            for (int i = 0; i < Slots.Length; ++i) {
                if (Slots[i].Occupant != null)
                    keys.Add(Slots[i].Occupant);
            }
            return keys;
        }

        /// <inheritdoc/>
        public override HashSet<FieldSlot> GetEmptySlots() {
            var keys = new HashSet<FieldSlot>();
            for (int i = 0; i < Slots.Length; ++i) {
                if (Slots[i].Occupant == null)
                    keys.Add(Slots[i]);
            }
            return keys;
        }

        /// <inheritdoc/>
        public override FieldEnumerator GetEnumerator() => new FieldEnumerator(Slots);

        /// <inheritdoc/>
        public override int SlotIndex(FieldSlot slot) {
            int i = 0;
            foreach (var slotCheck in Slots) {
                if (slot == slotCheck)
                    return i;
                ++i;
            }
            return ERR_SLOT;
        }
    }
}
