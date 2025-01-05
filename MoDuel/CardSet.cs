using MoDuel.Cards;
using MoDuel.Players;

namespace MoDuel;

/// <summary>
/// A set that contains <see cref="CardInstance"/>s and counts as location for their position.
/// </summary>
public class CardSet : HashSet<CardInstance>, ILocation {

    /// <summary>
    /// The owner of the set.
    /// </summary>
    public readonly Player? Owner;

    /// <summary>
    /// The identifier used to define this set as a location and indicate which type of location.
    /// </summary>
    public readonly string? LocationCode;

    /// <param name="owner">An owner designated to the set.</param>
    /// <param name="locationCode"></param>
    public CardSet(Player? owner, string? locationCode = null) {
        Owner = owner;
        LocationCode = locationCode;
    }

    /// <summary>
    /// Add a card to the set if it was not already in the set.
    /// </summary>
    public void AddToLocation(CardInstance card) {
        if (!Contains(card)) {
            card.Position = this;
        }
    }

    /// <summary>
    /// Remove a card from the set if it is in the set.
    /// </summary>
    public bool RemoveFromLocation(CardInstance card) {
        if (Contains(card)) {
            card.Position = null;
        }
        return false;
    }

    /// <inheritdoc/>
    public void JustRemove(CardInstance card) {
        Remove(card);
    }

    /// <inheritdoc/>
    public void JustAdd(CardInstance card) {
        Add(card);
    }

    /// <summary>
    /// Get the player that owns the set. Null if it is not owned.
    /// </summary>
    /// <returns></returns>
    public Player? GetOwner() => Owner;

    /// <summary>
    /// Remove all cards from the cards set.
    /// </summary>
    public new void Clear() {
        foreach (var card in this.ToArray()) {
            card.Position = null;
        }
    }

}
