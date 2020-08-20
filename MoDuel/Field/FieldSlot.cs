using MoDuel.Cards;

namespace MoDuel.Field {

    /// <summary>
    /// A Slot on a field that may contain a creature.
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class FieldSlot : Target {

        /// <summary>
        /// The field this slot is in.
        /// </summary>
        public SubField ParentField = null;

        public CreatureInstance _occupant = null;
        /// <summary>
        /// A <see cref="CreatureInstance"/> that is in the slot.
        /// <para>null if the slot is empty.</para>
        /// </summary>
        public CreatureInstance Occupant {
            get { return _occupant; }
            set {
                if (value != null) {
                    if (value._position != null)
                        value._position._occupant = null;
                    value._position = this;
                }
                //If the creature is null we still need to clear the occupant.
                else {
                    if (_occupant != null)
                        _occupant._position = null;
                }

                _occupant = value;
            }
        }


        /// <summary>
        /// Check to see if the slot is unoccupied.
        /// </summary>
        public bool IsEmpty => Occupant == null;

        /// <summary>
        /// Check to see if the slot is occupied.
        /// </summary>
        public bool IsOccupied => Occupant != null;

    }
}
