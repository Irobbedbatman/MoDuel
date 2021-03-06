using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoDuel.Cards;
using System.Collections;

namespace MoDuel.Field {

    /// <summary>
    /// A Battle Field that has accessors to creatures.
    /// <para>Without using <see cref="SubField"/> no creatures are actually stored.</para>
    /// <para>Can be Targeted and has a unique <see cref="Target.TargetIndex"/></para>
    /// </summary>
    [MoonSharpUserData]
    public abstract class Field : Target, IEnumerable<FieldSlot> {

        public const int ERR_SLOT = -1;

        /// <summary>
        /// Accessor for a given position on the field.
        /// </summary>
        /// <param name="index">The index of the field.</param>
        /// <returns>The <see cref="FieldSlot"/> at the index.</returns>
        public abstract FieldSlot this[int index] { get; }

        /// <summary>
        /// How many creatures are on the field.
        /// </summary>
        public int Count => GetCreatures().Count();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<FieldSlot> IEnumerable<FieldSlot>.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="IEnumerator"/> for iterating over a field.
        /// </summary>
        public abstract FieldEnumerator GetEnumerator();

        /// <summary>
        /// Gets any creatures contained within the field.
        /// </summary>
        /// /// <returns>An array of <see cref="CreatureInstance"/> that on the field.</returns>
        public abstract HashSet<CreatureInstance> GetCreatures();

        /// <summary>
        /// Gets any slots that dont have a occupant.
        /// </summary>
        /// <returns>An array of all the empty <see cref="FieldSlot"/> in this <see cref="Field"/></returns>
        public abstract HashSet<FieldSlot> GetEmptySlots();

        /// <summary>
        /// Gets the index the slot is at in the field.
        /// </summary>
        /// <param name="slot">The slot to get an index of.</param>
        /// <returns>The slots positional index.</returns>
        public abstract int SlotIndex(FieldSlot slot);

    }
}
