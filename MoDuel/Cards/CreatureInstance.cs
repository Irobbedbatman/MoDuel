using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Field;

namespace MoDuel.Cards {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class CreatureInstance : CardInstance {

        /// <summary>
        /// One of the current stats for this creature.
        /// </summary>
        public int CurrentLife, CurrentAttack, CurrentArmor, MaxLife;

        public FieldSlot _position;
        /// <summary>
        /// The <see cref="FieldSlot"/> this creature occupies.
        /// </summary>
        public FieldSlot Position {
            get {
                return _position;
            }
            set {
                if (value != null) {
                    if (value._occupant != null)
                        value._occupant._position = null;
                    value._occupant = this;
                    CurrentOwner = value.ParentField.Owner;
                }
                //If the new slot is null we still need to clear the position.
                else {
                    if (_position != null) {
                        _position.Occupant = null;
                    }
                    CurrentOwner = null;
                }
                _position = value;
            }
        }

        public CreatureInstance(Card imprint, CardInstanceActivator activator) : base(imprint, activator) {
            Position = null;
        }

        public CreatureInstance(Card imprint, CardInstanceActivator activator, FieldSlot position) : base(imprint, activator) {
            Position = position;
        }

        public CreatureInstance(CardInstance card, CardInstanceActivator activator, FieldSlot position) : base(card.Imprint, activator, card) {
            Position = position;
        }

        public CreatureInstance(Card imprint, CardInstanceActivator activator, CreatureInstance previousState) : base(imprint, activator, previousState) {
            if (previousState.Position != null)
                previousState.Position.Occupant = this;
        }

        /// <summary>
        /// Check to see if this creature is alive as it has a position. (is on the field.)
        /// </summary>
        public new bool IsAlive {
            get {
                if (Position == null)
                    return false;
                return true;
            }
        }
    }
}
