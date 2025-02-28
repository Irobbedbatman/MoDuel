using MoDuel.Cards;
using MoDuel.Players;
using MoDuel.Serialization;

namespace MoDuel.Field;

/// <summary>
/// A <see cref="Field"/> that stores <seealso cref="CardInstance"/>s inside an array <seealso cref="Slots"/>
/// </summary>
[SerializeReference]
public class SubField : Field {

    /// <summary>
    /// The <see cref="Player"/> that this <see cref="SubField"/> is owned by.
    /// </summary>
    public readonly Player Owner;

    /// <summary>
    /// The slots in this field; each a <see cref="FieldSlot"/>.
    /// </summary>
    public readonly FieldSlot[] Slots = new FieldSlot[5];

    /// <summary>
    /// The <see cref="FullField"/> this <see cref="SubField"/> is within.
    /// </summary>
    public FullField FullField => Context.Field;

    public SubField(Player owner) : base(owner.Context) {
        Slots = [new(this, 0), new(this, 1), new(this, 2), new(this, 3), new(this, 4)];
        Owner = owner;
    }

    /// <inheritdoc/>
    public override FieldSlot this[int position] => Slots[position - 1];

    public override int SlotCount => Slots.Length;

    /// <inheritdoc/>
    public override HashSet<CardInstance> GetCreatures() {
        var keys = new HashSet<CardInstance>();
        for (int i = 0; i < Slots.Length; ++i) {
            var occupant = Slots[i].Occupant;
            if (occupant != null)
                keys.Add(occupant);
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
    public override IEnumerator<FieldSlot> GetEnumerator() => Slots.AsEnumerable().GetEnumerator();

    /// <inheritdoc/>
    public override int SlotPosition(FieldSlot slot) {
        if (slot.ParentField == this)
            return slot.RelativePosition;
        return ERR_SLOT;
    }

    /// <inheritdoc/>
    public override int WrapHorizontal(int origin, int move) {
        int newPos = origin + move;
        while (newPos < 0)
            newPos += SlotCount - 1;
        while (newPos >= SlotCount)
            newPos -= SlotCount;
        return newPos;
    }

    /// <inheritdoc/>
    public override int MoveHorizontal(int origin, int move) {
        int newPos = origin + move;
        if (newPos < 0)
            newPos = 0;
        if (newPos >= SlotCount)
            newPos = SlotCount;
        return newPos;
    }

    /// <inheritdoc/>
    public override int? Move(int origin, int move) {
        int newPos = origin + move;
        if (newPos < 0)
            return null;
        if (newPos >= SlotCount)
            return null;
        return newPos;
    }

    /// <summary>
    /// Returns which position this is with the <see cref="FullField"/>.
    /// <para>The value is <see cref="ERR_SLOT"/> if there is an error.</para>
    /// </summary>
    public int Position {
        get {
            if (FullField.SubField1 == this) return 0;
            if ((FullField.SubField2 == this)) return 1;
            return ERR_SLOT;
        }
    }

}
