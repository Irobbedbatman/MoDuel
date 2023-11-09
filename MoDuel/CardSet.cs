using MoDuel.Cards;
using MoDuel.Players;
using System.Collections;

namespace MoDuel;

/// <summary>
/// A set that contains <see cref="CardInstance"/>s and counts as location for their position.
/// </summary>
/// <param name="owner">An owner designated to the set.</param>
public class CardSet(Player? owner) : ICollection<CardInstance>, ILocation {

    /// <summary>
    /// The internal dta set.
    /// </summary>
    private readonly HashSet<CardInstance> CardInstances = [];

    /// <summary>
    /// The owner of the set.
    /// </summary>
    public readonly Player? Owner = owner;

    /// <summary>
    /// THe amount of cards contained within the set.
    /// </summary>
    public int Count => CardInstances.Count;

    /// <summary>
    /// Card sets are never readonly.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Add a card to the set if it was not already in the set.
    /// </summary>
    public void Add(CardInstance card) {
        if (!CardInstances.Contains(card)) {
            card.Position = this;
        }
    }

    /// <summary>
    /// Remove a card from the set if it is in the set.
    /// </summary>
    public bool Remove(CardInstance card) {
        if (Contains(card)) {
            card.Position = null;
        }
        return false;
    }

    /// <inheritdoc/>
    public void JustRemove(CardInstance card) {
        CardInstances.Remove(card);
    }

    /// <inheritdoc/>
    public void JustAdd(CardInstance card) {
        CardInstances.Add(card);
    }

    /// <summary>
    /// Get the player that owns the set. Null if it is not owned.
    /// </summary>
    /// <returns></returns>
    public Player? GetOwner() => Owner;

    /// <summary>
    /// Remove all cards from the cards set.
    /// </summary>
    public void Clear() {
        foreach (var card in CardInstances.ToArray()) {
            card.Position = null;
        }
    }

    /// <summary>
    /// Check to see if the provided card is in the set.
    /// </summary>
    public bool Contains(CardInstance item) => CardInstances.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(CardInstance[] array, int arrayIndex) {
        array[arrayIndex] = CardInstances.ElementAt(arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<CardInstance> GetEnumerator() => CardInstances.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => CardInstances.GetEnumerator();

}
