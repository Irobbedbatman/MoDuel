using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Cards;
using MoonSharp.Interpreter;

namespace MoDuel.Field {
    /// <summary>
    /// A field that combines 2 <see cref="SubField"/>
    /// <para>Accessors can index both <see cref="SubField"/>s as if they were a single <see cref="Field"/></para>
    /// </summary>
    [MoonSharpUserData]
    public class FullField : Field {

        /// <summary>
        /// The <see cref="SubField"/> that starts at index 0 and proceeds to 4 in any index based method.
        /// </summary>
        public readonly SubField Near;
        /// <summary>
        /// The <see cref="SubField"/> that starts at index 5 and proceeds to 9 in any index based method.
        /// </summary>
        public readonly SubField Far;

        public FullField(SubField near, SubField far) {
            Near = near;
            Far = far;
        }

        /// <summary>
        /// Wraps a postion around a sub field after it has moved.
        /// <para>Waps both negative movement and positive movement.</para>
        /// <para>Invalid origin or move will result in a non-wrapped number.</para>
        /// </summary>
        /// <param name="origin">The starting point.</param>
        /// <param name="move">The direction and amount moving. Use negative to go left. Cant be greater than 5</param>
        /// <returns>The new wrapped around position.</returns>
        public static int Wrap(int origin, int move) {
            if (origin < 0 || origin > 9 || move < -5 || move > 5)
                return origin + move;
            //Get the wrapped position.
            int newPos = (origin + move) % 5;
            //If on the far sub field we get higher values.
            if (origin >= 5)
                return newPos < 0 ? newPos + 10 : newPos + 5;
            //If move resulted in a negative value we add 5.
            return newPos < 0 ? newPos + 5 : newPos;
        }

        /// <summary>
        /// Gets the fieldSlot id of the slot accoss from the given position.
        /// </summary>
        /// <param name="position">The position to check from.</param>
        /// <returns>The field slot accross from <paramref name="position"/>.</returns>
        public static int Opposing(int position) => (position < 5) ? position + 5 : position - 5;

        /// <inheritdoc/>
        public override FieldSlot this[int index] => index >= 5 ? Far[index - 5] : Near[index];
        /// <inheritdoc/>
        public override HashSet<CreatureInstance> GetCreatures(int offset = 0) => Near.GetCreatures().Concat(Far.GetCreatures(offset: 5)).ToHashSet();
        /// <inheritdoc/>
        public override HashSet<FieldSlot> GetEmptySlots(int offset = 0) => Near.GetEmptySlots().Concat(Far.GetEmptySlots(offset: 5)).ToHashSet();
        /// <inheritdoc/>
        public override FieldEnumerator GetEnumerator() => new FieldEnumerator(Near.Slots.Concat(Far.Slots).ToArray());
        /// <inheritdoc/>
        public override int SlotIndex(FieldSlot slot) {
            if (Near.Slots.Contains(slot))
                return Near.SlotIndex(slot);
            else if (Far.Slots.Contains(slot))
                return Far.SlotIndex(slot) + 5;
            else
                return ERR_SLOT;
        }



    }
}
