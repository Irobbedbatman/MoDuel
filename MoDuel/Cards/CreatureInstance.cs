using MoDuel.Field;
using MoDuel.Serialization;
using MoDuel.State;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel.Cards;

/// <summary>
/// An instantiated version of a <see cref="Card"/> that allows it for it to have a postion and a state on a field.
/// <para>Can be created from a <see cref="CardInstance"/.></para>
/// </summary>
[SerializeReference]
public class CreatureInstance : CardInstance {

    /// <summary>
    /// One of the current stats of this card.
    /// </summary>
    public int Level, Life, Attack, Armour, MaxLife;

    /// <summary>
    /// The internal position value used sync or desync with a <see cref="FieldSlot.Occupant"/> value.
    /// </summary>
    internal FieldSlot? _position;

    /// <summary>
    /// The <see cref="FieldSlot"/> this creature occupies.
    /// </summary>
    public FieldSlot? Position {
        get {
            return _position;
        }
        set {
            if (value != null) {
                // Remove the old occupant.
                if (value._occupant != null) {
                    value._occupant.Position = null;
                }
                value._occupant = this;
                Owner = value.ParentField.Owner;
            }
            //If the new slot is null we still need to clear the position.
            else {
                if (_position != null) {
                    _position.Occupant = null;
                }
                Owner = null;
            }
            _position = value;
        }
    }

    /// <summary>
    /// Create a new creature that was not created froma card.
    /// </summary>
    public CreatureInstance(DuelState context, CardMeta meta) : base(context, meta) { }

    /// <summary>
    /// Create a creature from a card instance but chanegs its meta data.
    /// </summary>
    public CreatureInstance(CardMeta meta, CardInstance previousState) : base(meta, previousState) { }

    /// <summary>
    /// Create a creature from a card instance.
    /// </summary>
    public CreatureInstance(CardInstance previousState) : base(previousState.Meta, previousState) { }

    /// <summary>
    /// Setter for <see cref="Position"/> which returns the creature itself.
    /// </summary>
    public CreatureInstance SetPosition(FieldSlot position) {
        Position = position;
        return this;
    }

    /// <summary>
    /// Check to see if this creature is alive as it has a position. (is on the field.)
    /// </summary>
    [MemberNotNullWhen(true, nameof(Position), nameof(_position))]
    public override bool IsAlive => Position != null;

}
