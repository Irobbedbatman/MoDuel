using MoDuel.Cards;
using MoDuel.Serialization;
using MoDuel.State;

namespace MoDuel.Field;

/// <summary>
/// A field that combines 2 <see cref="SubField"/>
/// <para>Accessors can index both <see cref="SubField"/>s as if they were a single <see cref="Field"/></para>
/// </summary>
[SerializeReference]
public class FullField : Field {

    /// <summary>
    /// The <see cref="SubField"/> that starts at index 0 and proceeds to 4 in any index based method.
    /// </summary>
    public readonly SubField SubField1;
    /// <summary>
    /// The <see cref="SubField"/> that starts at index 5 and proceeds to 9 in any index based method.
    /// </summary>
    public readonly SubField SubField2;
    /// <summary>
    /// The set of columns. Should have a length of five.
    /// </summary>
    public readonly FieldColumn[] Columns;

    public override int SlotCount => SubField1.SlotCount + SubField2.SlotCount;

    public FullField(DuelState context, SubField subfield1, SubField subfield2) : base(context) {
        SubField1 = subfield1;
        SubField2 = subfield2;
        Columns = [
            new FieldColumn(this, 1),
            new FieldColumn(this, 2),
            new FieldColumn(this, 3),
            new FieldColumn(this, 4),
            new FieldColumn(this, 5)
        ];
    }

    /// <inheritdoc/>
    public override int WrapHorizontal(int origin, int move) {
        if (origin <= 5)
            return SubField1.WrapHorizontal(origin, move);
        else
            return SubField2.WrapHorizontal(origin - 5, move) + 5;
    }

    /// <inheritdoc/>
    public override int MoveHorizontal(int origin, int move) {
        if (origin <= 5)
            return SubField1.MoveHorizontal(origin, move);
        else
            return SubField2.MoveHorizontal(origin - 5, move) + 5;
    }


    /// <inheritdoc/>
    public override int? Move(int origin, int move) {
        if (origin <= 5)
            return SubField1.Move(origin, move);
        else
            return SubField2.Move(origin - 5, move) + 5;
    }

    /// <summary>
    /// Gets the fieldSlot id of the slot access from the given position.
    /// </summary>
    /// <param name="position">The position to check from.</param>
    /// <returns>The field slot across from <paramref name="position"/>.</returns>
    public static int GetOpposingPosition(int position) => (position <= 5) ? position + 5 : position - 5;

    /// <inheritdoc/>
    public override FieldSlot this[int position] => (position > 5) ? SubField2[position - 5] : SubField1[position];
    /// <inheritdoc/>
    public override HashSet<CardInstance> GetCreatures() => [.. SubField1.GetCreatures(), .. SubField2.GetCreatures()];
    /// <inheritdoc/>
    public override HashSet<FieldSlot> GetEmptySlots() => [.. SubField1.GetEmptySlots(), .. SubField2.GetEmptySlots()];
    /// <inheritdoc/>
    public override IEnumerator<FieldSlot> GetEnumerator() => SubField1.Concat(SubField2).GetEnumerator();
    /// <inheritdoc/>
    public override int SlotPosition(FieldSlot slot) {
        if (SubField1.Slots.Contains(slot))
            return SubField1.SlotPosition(slot);
        else if (SubField2.Slots.Contains(slot))
            return SubField2.SlotPosition(slot) + 5;
        else
            return ERR_SLOT;
    }

}
