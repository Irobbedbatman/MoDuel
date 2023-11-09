using MoDuel.Cards;
using MoDuel.Serialization;
using MoDuel.State;
using System.Collections;

namespace MoDuel.Field;

/// <summary>
/// A Battle Field that has accessors to creatures.
/// <para>Without using <see cref="SubField"/> no creatures are actually stored.</para>
/// <para>Can be Targeted and has a unique <see cref="Target.TargetIndex"/></para>
/// </summary>
[SerializeReference]
public abstract class Field(DuelState context) : Target(context.TargetRegistry), IEnumerable<FieldSlot> {

    /// <summary>
    /// The slot index for a slot that hasn't been provided.
    /// </summary>
    public const int ERR_SLOT = -1;

    /// <summary>
    /// The contextual state that this field exits within.
    /// </summary>
    public readonly DuelState Context = context;

    /// <summary>
    /// Accessor for a given position on the field.
    /// </summary>
    /// <param name="position">The position of the field slot.</param>
    /// <returns>The <see cref="FieldSlot"/> at the index.</returns>
    public abstract FieldSlot this[int position] { get; }

    /// <summary>
    /// How many slots are a part of the field.
    /// </summary>
    public abstract int SlotCount { get; }

    /// <summary>
    /// How many creatures are on the field.
    /// </summary>
    public int CreatureCount => GetCreatures().Count;

    /// <summary>
    /// Gets any creatures contained within the field.
    /// </summary>
    /// /// <returns>An array of <see cref="CardInstance"/> that on the field.</returns>
    public abstract HashSet<CardInstance> GetCreatures();

    /// <summary>
    /// Gets any slots that don't have a occupant.
    /// </summary>
    /// <returns>An array of all the empty <see cref="FieldSlot"/> in this <see cref="Field"/></returns>
    public abstract HashSet<FieldSlot> GetEmptySlots();

    /// <inheritdoc/>
    public abstract IEnumerator<FieldSlot> GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the index the slot is at in the field.
    /// </summary>
    /// <param name="slot">The slot to get the slot position</param>
    /// <returns>The position of this slot in the field.</returns>
    public abstract int SlotPosition(FieldSlot slot);

    /// <summary>
    /// Move from an <paramref name="origin"/> index and if necessary wrap horizontally.
    /// <para>Ensure origin is a valid position as <see cref="FullField"/> will not be able to wrap correctly otherwise.</para>
    /// </summary>
    /// <param name="origin">The original position to move from.</param>
    /// <param name="move">The positive or negative movement horizontally.</param>
    /// <returns>The index of that represents the position. Whether wrapping occurred or not.</returns>
    public abstract int WrapHorizontal(int origin, int move);

    /// <summary>
    /// Move from an <paramref name="origin"/> index but do not attempt wrapping.
    /// <para>Ensure origin is a valid position.</para>
    /// </summary>
    /// <param name="origin">The original position to move from.</param>
    /// <param name="move">The positive or negative movement horizontally.</param>
    /// <returns>The index of that represents the position. The position is clamped to the left or right edge.</returns>
    public abstract int MoveHorizontal(int origin, int move);

    /// <summary>
    /// Move from an <paramref name="origin"/>.
    /// </summary>
    /// <param name="origin">The original position to move from.</param>
    /// <param name="move">The positive or negative movement horizontally.</param>
    /// <returns>A valid position after the movement or null if that movement is impossible.</returns>
    public abstract int? Move(int origin, int move);

    /// <summary>
    /// Check to see if the position provided falls within the field.
    /// </summary>
    public bool IsValidPosition(int position) => position >= 1 && position <= SlotCount;


}
