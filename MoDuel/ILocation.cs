using MoDuel.Cards;
using MoDuel.Players;

namespace MoDuel;

/// <summary>
/// A location <see cref="CardInstance"/>s are placed within. 
/// </summary>
public interface ILocation {

    /// <summary>
    /// For locations that can only have one remove the card that occupies that location.
    /// </summary>
    public void RemoveCurrentOccupant() { }

    /// <summary>
    /// Remove the card from the location without updating the card's position.
    /// </summary>
    /// <param name="card"></param>
    public void JustRemove(CardInstance card);

    /// <summary>
    /// Add the card to the location without updating the card's position.
    /// </summary>
    public void JustAdd(CardInstance card);

    /// <summary>
    /// Get the owner of this location.
    /// </summary>
    public Player? GetOwner();

}
