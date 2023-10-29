using MoDuel.Cards;
using MoDuel.Serialization;
using MoDuel.State;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel.Field;

/// <summary>
/// A Slot on a field that may contain a creature.
/// </summary>
[SerializeReference]
public class FieldSlot : Target {

    /// <summary>
    /// The field this slot is in.
    /// </summary>
    public readonly SubField ParentField;

    /// <summary>
    /// The <see cref="FullField"/> this slot is in.
    /// </summary>
    public FullField FullField => ParentField.FullField;

    /// <summary>
    /// The position this <see cref="FieldSlot"/> in retlation to it's <see cref="ParentField"/>.
    /// <para>If trying to find the position of this slot in a <see cref="FullField"/> use <see cref="Field.SlotPosition(FieldSlot)"/>.</para>
    /// </summary>
    public readonly int RelativePosition;

    /// <summary>
    /// The positoin of this <see cref="FieldSlot"/> within the <see cref="FullField"/>.
    /// </summary>
    public int FullPosition => FullField.SlotPosition(this);

    /// <summary>
    /// The internal field to sync and esync with <see cref="CreatureInstance.Position"/>
    /// </summary>
    internal CreatureInstance? _occupant = null;

    /// <summary>
    /// A <see cref="CreatureInstance"/> that is in the slot.
    /// <para>null if the slot is empty.</para>
    /// </summary>
    public CreatureInstance? Occupant {
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
    /// The context of this <see cref="FieldSlot"/> derived through it's <see cref="SubField"/>.
    /// </summary>
    public DuelState Context => ParentField.Context;

    public FieldSlot(SubField parent, int relativePosition) : base(parent.Context.TargetRegistry) {
        ParentField = parent;
        RelativePosition = relativePosition;
    }

    /// <summary>
    /// Check to see if the slot is unoccupied.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Occupant))]
    public bool IsEmpty => Occupant == null;

    /// <summary>
    /// Check to see if the slot is occupied.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Occupant))]
    public bool IsOccupied => Occupant != null;

    /// <summary>
    /// The <see cref="FieldColumn"/> this slot is in.
    /// </summary>
    public FieldColumn Column => FullField.Columns[RelativePosition - 1];

    /// <summary>
    /// Get's the slot opposing this slot.
    /// </summary>
    public FieldSlot GetOpposingSlot() => FullField[FullField.GetOpposingPosition(FullPosition)];

    /// <summary>
    /// Gets the position to the right of this <see cref="FieldSlot"/>.
    /// </summary>
    /// <returns>Null if the there is no slot to the right.</returns>
    public FieldSlot? GetSlotToTheRight() {
        var position = ParentField.Move(RelativePosition, 1);
        if (position == null) return null;
        return ParentField[position.Value];
    }


    /// <summary>
    /// Gets the position to the left of this <see cref="FieldSlot"/>.
    /// </summary>
    /// <returns>Null if the there is no slot to the left.</returns>
    public FieldSlot? GetSlotToTheLeft() {
        var position = ParentField.Move(RelativePosition, -1);
        if (position == null) return null;
        return ParentField[position.Value];
    }


    /// <summary>
    /// Gets the slot ot the right. If the ther is no slot to the right will wrap around the field.
    /// </summary>
    public FieldSlot GetSlotToTheRightWithWrapping() {
        var position = ParentField.WrapHorizontal(RelativePosition, 1);
        return ParentField[position];
    }


    /// <summary>
    /// Gets the slot ot the left. If the ther is no slot to the left will wrap around the field.
    /// </summary>
    public FieldSlot GetSlotToTheLeftWithWrapping() {
        var position = ParentField.WrapHorizontal(RelativePosition, -1);
        return ParentField[position];
    }
}
