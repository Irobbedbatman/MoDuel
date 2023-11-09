using MoDuel.Serialization;
using MoDuel.State;

namespace MoDuel.Field;

/// <summary>
/// Represent a column created by two <see cref="FieldSlot"/>s of a <see cref="FullField"/>.<br/>
/// With a slot for each <see cref="SubField"/>.
/// </summary>
[SerializeReference]
public class FieldColumn(FullField field, int pos) : Target(field.Context.TargetRegistry) {

    /// <summary>
    /// The <see cref="FullField"/> that this column is created from.
    /// </summary>
    public readonly FullField Field = field;
    /// <summary>
    /// The <see cref="FieldSlot"/> on the <see cref="DuelState.Player1"/> side of this <see cref="FieldColumn"/>.
    /// </summary>
    public readonly FieldSlot Slot1 = field.SubField1[pos];
    /// <summary>
    /// The <see cref="FieldSlot"/> on the <see cref="DuelState.Player2"/> side of this <see cref="FieldColumn"/>.
    /// </summary>
    public readonly FieldSlot Slot2 = field.SubField2[pos];
    /// <summary>
    /// What position this column is in within the <see cref="FullField"/>.
    /// </summary>
    public readonly int Position = pos;
    /// <summary>
    /// The context for the column derived from the <see cref="Field"/> that it was created from.
    /// </summary>
    public DuelState Context => Field.Context;
}
