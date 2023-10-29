using MoDuel.Serialization;
using MoDuel.State;

namespace MoDuel.Field;

/// <summary>
/// Represent a column created by two <see cref="FieldSlot"/>s of a <see cref="FullField"/>.<br/>
/// With a slot for each <see cref="SubField"/>.
/// </summary>
[SerializeReference]
public class FieldColumn : Target {

    /// <summary>
    /// The <see cref="FullField"/> that this column is created from.
    /// </summary>
    public readonly FullField Field;
    /// <summary>
    /// The <see cref="FieldSlot"/> on the <see cref="DuelState.Player1"/> side of this <see cref="FieldColumn"/>.
    /// </summary>
    public readonly FieldSlot Slot1;
    /// <summary>
    /// The <see cref="FieldSlot"/> on the <see cref="DuelState.Player2"/> side of this <see cref="FieldColumn"/>.
    /// </summary>
    public readonly FieldSlot Slot2;
    /// <summary>
    /// What position this column is in within the <see cref="FullField"/>.
    /// </summary>
    public readonly int Position;
    /// <summary>
    /// The context for the column derived from the <see cref="Field"/> that it was created from.
    /// </summary>
    public DuelState Context => Field.Context;

    public FieldColumn(FullField field, int pos) : base(field.Context.TargetRegistry) {
        Field = field;
        Position = pos;
        Slot1 = field.SubField1[pos];
        Slot2 = field.SubField2[pos];
    }

}
