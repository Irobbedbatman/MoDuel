using MoDuel.Cards;
using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.State;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel.Field;

/// <summary>
/// A Slot on a field that may contain a creature.
/// </summary>
[SerializeReference]
public class FieldSlot(SubField parent, int relativePosition) : Target(parent.Context.TargetRegistry), ILocation {

    /// <summary>
    /// The field this slot is in.
    /// </summary>
    public readonly SubField ParentField = parent;

    /// <summary>
    /// The <see cref="FullField"/> this slot is in.
    /// </summary>
    public FullField FullField => ParentField.FullField;

    /// <summary>
    /// The position this <see cref="FieldSlot"/> in relation to it's <see cref="ParentField"/>.
    /// <para>If trying to find the position of this slot in a <see cref="FullField"/> use <see cref="Field.SlotPosition(FieldSlot)"/>.</para>
    /// </summary>
    public readonly int RelativePosition = relativePosition;

    /// <summary>
    /// The position of this <see cref="FieldSlot"/> within the <see cref="FullField"/>.
    /// </summary>
    public int FullPosition => FullField.SlotPosition(this);

    /// <summary>
    /// The internal field to sync with <see cref="CardInstance.Position"/>
    /// </summary>
    internal CardInstance? _occupant = null;

    /// <summary>
    /// A <see cref="CardInstance"/> that is in the slot.
    /// <para>null if the slot is empty.</para>
    /// </summary>
    /// 
    [MemberNotNullWhen(true, nameof(_occupant))]
    public CardInstance? Occupant {
        get { return _occupant; }
        set {
            // Don't bother setting when value is the same.
            if (_occupant == value) return;
            // Clear the position for the current occupant. This will clear this position too.
            if (_occupant != null) {
                _occupant.Position = null;
            }
            // If a new card is provided; update it's position. This will also update the occupant.
            if (value != null) {
                value.Position = this;
            }
            // Otherwise mark the location as empty.
            else {
                _occupant = null;
            }
        }
    }

    /// <summary>
    /// The context of this <see cref="FieldSlot"/> derived through it's <see cref="SubField"/>.
    /// </summary>
    public DuelState Context => ParentField.Context;

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
    /// Gets the slot ot the right. If there is no slot to the right will wrap around the field.
    /// </summary>
    public FieldSlot GetSlotToTheRightWithWrapping() {
        var position = ParentField.WrapHorizontal(RelativePosition, 1);
        return ParentField[position];
    }


    /// <summary>
    /// Gets the slot ot the left. If there is no slot to the left will wrap around the field.
    /// </summary>
    public FieldSlot GetSlotToTheLeftWithWrapping() {
        var position = ParentField.WrapHorizontal(RelativePosition, -1);
        return ParentField[position];
    }

    /// <inheritdoc/>
    public void RemoveCurrentOccupant() { 
        Occupant = null;
    }

    /// <inheritdoc/>
    public void JustRemove(CardInstance card) {
        _occupant = null;
    }

    /// <inheritdoc/>
    public void JustAdd(CardInstance card) {
        _occupant = card;
    }

    /// <inheritdoc/>
    public Player GetOwner() => ParentField.Owner;

}
