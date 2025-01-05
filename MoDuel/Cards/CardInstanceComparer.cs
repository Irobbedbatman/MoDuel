using MoDuel.Serialization;
using MoDuel.State;
using MoDuel.Triggers;

namespace MoDuel.Cards;

/// <summary>
/// Comparer that can be used to get activation priority of <see cref="CardInstance"/>s.
/// <para>Lower values indicate higher priority.</para>
/// </summary>
[SerializeReference]
public class CardInstanceComparer : IComparer<CardInstance> {

    /// <summary>
    /// The trigger that the <see cref="CardInstance"/>s are reacting to.
    /// </summary>
    public readonly Trigger Trigger;

    /// <summary>
    /// Get the state the trigger was called within.
    /// </summary>
    public DuelState State => Trigger.State;

    public CardInstanceComparer(Trigger trigger) {
        Trigger = trigger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int Compare(CardInstance? x, CardInstance? y) {

        // Null check
        if (x == null || y == null)
            return 0;

        // Creatures have more priority than non-creatures.
        int compareCreatures = CompareCreatures(x, y);
        if (compareCreatures != 0)
            return compareCreatures;

        // Compare owners.
        int ownerCompare = CompareOwner(x, y);
        if (ownerCompare != 0)
            return ownerCompare;

        // Prioritize cards in hand.
        int handCompare = CompareInHand(x, y);
        if (handCompare != 0)
            return handCompare;

        // Prioritize cards in the grave.
        int graveCompare = CompareInGrave(x, y);
        if (graveCompare != 0)
            return graveCompare;

        // Finally prioritize by id which will guarantee a consistent ordering.
        int indexCompare = x.Imprint.Id.CompareTo(y.Imprint.Id);
        return indexCompare;
    }


    /// <summary>
    /// Compares two <see cref="CardInstance"/>s to see if one is in the hand and one isn't.
    /// <para>If both cards are in the hand or both are not return 0.</para>
    /// </summary>
    /// <returns>-1 if x is the only card in the hand. +1 if y is the only card in the hand. 0 otherwise.</returns>
    private static int CompareInHand(CardInstance x, CardInstance y) {
        if (x.InHand && !y.InHand)
            return -1;
        if (!y.InHand && y.InHand)
            return 1;
        return 0;
    }

    /// <summary>
    /// Compares two <see cref="CardInstance"/>s to see if one is in the grave and one isn't.
    /// <para>If both cards are in the grave or both are not return 0.</para>
    /// </summary>
    /// <returns>-1 if x is the only card in the grave. +1 if y is the only card in the grave. 0 otherwise.</returns>
    private static int CompareInGrave(CardInstance x, CardInstance y) {
        if (x.InGrave && !y.InGrave)
            return -1;
        if (!x.InGrave && y.InGrave)
            return 1;
        return 0;
    }

    /// <summary>
    /// Compares 2 <see cref="CardInstance"/>s. Prioritising cards that alive.
    /// <para>If both creatures are alive: compares by owner.</para>
    /// <para>If both creatures have the same owner compares their position.</para>
    /// </summary>
    /// <returns>
    /// Returns 0 if neither creature is alive.<br/>
    /// -1 if <paramref name="x"/> is the only card alive. +1 if <paramref name="y"/> is the only card alive. Otherwise compares based of owner.<br/>
    /// If both cards have the same owner compares position.
    /// </returns>
    private int CompareCreatures(CardInstance x, CardInstance y) {

        // Prioritize creatures that are alive.
        if (x.IsAlive && !y.IsAlive)
            return -1;

        if (!x.IsAlive && y.IsAlive)
            return +1;

        // If neither card is alive can't compare the creatures.
        if (!x.IsAlive || !y.IsAlive)
            return 0;

        // Compare owner is used to check if cards are on the same side of the field.
        int ownerCompare = CompareOwner(x, y);

        // If both cards have different owners use the result.
        if (ownerCompare != 0)
            return ownerCompare;

        // Compare creature positions. Cards with lower relative position have higher priority.
        var positionCompare = 0;
        positionCompare -= x.FieldPosition.RelativePosition;
        positionCompare += y.FieldPosition.RelativePosition;

        return positionCompare;

    }

    /// <summary>
    /// Compares two <see cref="CardInstance"/>s to see if their owner is the current turn owner.
    /// </summary>
    /// <returns>-1 if x is owned by the current turn owner. +1 if y is the priority player. 0 otherwise.</returns>
    private int CompareOwner(CardInstance x, CardInstance y) {
        if (x.Owner == State.CurrentTurn.Owner)
            return -1;
        if (y.Owner == State.CurrentTurn.Owner)
            return 1;
        return 0;
    }

}
