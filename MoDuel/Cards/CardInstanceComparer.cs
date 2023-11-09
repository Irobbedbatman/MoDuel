using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.State;

namespace MoDuel.Cards;

/// <summary>
/// Comparer that can be used to get activation priority of <see cref="CardInstance"/>s and <see cref="CardInstance"/>s.
/// <para>Lower values indicate higher priority.</para>
/// </summary>
[SerializeReference]
public class CardInstanceComparer(Player priorityPlayer, string trigger = "", ActionFunction? customComparer = null) : IComparer<CardInstance> {

    /// <summary>
    /// The player whose cards have more priority than other players.
    /// </summary>
    public readonly Player PriorityPlayer = priorityPlayer;

    /// <summary>
    /// The <see cref="DuelState"/> that is requesting the comparison.
    /// </summary>
    public DuelState Context => PriorityPlayer.Context;

    /// <summary>
    /// A lua based comparer that is used first in <see cref="Compare(CardInstance, CardInstance)"/>.
    /// </summary>
    public readonly ActionFunction CustomComparer = customComparer ?? new ActionFunction();

    /// <summary>
    /// The trigger that the <see cref="CardInstance"/>s are reacting to.
    /// </summary>
    public readonly string Trigger = trigger;

    /// <summary>
    /// Compares two cards with the following priority.
    /// <list type="number">
    /// <item>Custom Explicit Comparison provided by <paramref name="x"/> and <paramref name="y"/>.</item>
    /// <item>Custom Comparison provided through <see cref="CustomComparer"/></item>
    /// <item>Creatures as per <see cref="CompareCreatures(CardInstance, CardInstance)"/>.</item>
    /// <item>Cards owned by <see cref="PriorityPlayer"/></item>
    /// <item>Cards in hand</item>
    /// <item>Cards in grave</item>
    /// <item>Cards with higher level.</item>
    /// <item>Alphabetical compared <see cref="Card.Id"/>.</item>
    /// </list>
    /// </summary>
    /// <param name="skipExplicit">Check to determine if Explicit conversion should be skipped. This can be a costly operation.</param>
    /// <returns>-1 for cards higher in priority. +1 for cards lower in priority. 0 for cards that are equivalent.</returns>
    public int Compare(CardInstance? x, CardInstance? y, bool skipExplicit = false) {

        if (!skipExplicit) {
            // Value of both explicit reactions checked against each other
            int compareImplicit = 0;
            if (x?.Imprint.ExplicitTriggerReactions.TryGetValue("Compare", out var value) ?? false) {
                int? result = value.Call(this, x, y);
                compareImplicit += result ?? 0;
            }
            if (y?.Imprint.ExplicitTriggerReactions.TryGetValue("Compare", out value) ?? false) {
                int? result = value.Call(this, y, x, Trigger);
                compareImplicit += result ?? 0;
            }
            // If implicit comparison is decisive return result.
            if (compareImplicit != 0)
                return compareImplicit;
        }

        // Use the provided custom convertor.
        int compareCustom = CustomComparer?.Call(this, x, y) ?? 0;
        if (compareCustom != 0)
            return compareCustom;

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

        // Prioritize cards with higher levels. 
        int levelCompare = y.GetDisplayedLevel()?.CompareTo(x.GetDisplayedLevel()) ?? 0;
        if (levelCompare != 0)
            return levelCompare;

        // Finally prioritize by index which will guarantee a consistent ordering.
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
    /// Compares two <see cref="CardInstance"/>s and their Owner against the <see cref="PriorityPlayer"/>.
    /// <para>If both cards are owned by the same player 0 is returned.</para>
    /// <para>If both cards don't have the <see cref="PriorityPlayer"/> 0 is returned.</para>
    /// </summary>
    /// <returns>-1 if x is owned by the <see cref="PriorityPlayer"/>. +1 if y is the priority player. 0 otherwise.</returns>
    private int CompareOwner(CardInstance x, CardInstance y) {
        if (x.Owner == y.Owner)
            return 0;
        if (x.Owner == PriorityPlayer)
            return -1;
        if (y.Owner == PriorityPlayer)
            return 1;
        return 0;
    }

    /// <inheritdoc/>
    public int Compare(CardInstance? x, CardInstance? y) => Compare(x, y, false);
}
