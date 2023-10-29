using MoDuel.Serialization;

namespace MoDuel.Cards;

/// <summary>
/// A set of values used to define special values for <see cref="CardInstance"/>s.
/// </summary>
[SerializeReference]
public class CardMeta {

    /// <summary>
    /// The player that owns the card.
    /// </summary>
    public readonly string? OwnerName;
    /// <summary>
    /// The base data that defines what the meta used in.
    /// </summary>
    public readonly Card Card;
    /// <summary>
    /// The raw meta data values.
    /// </summary>
    public readonly IReadOnlyDictionary<string, object> Values;

    public CardMeta(Card card, string? ownerName = null, IDictionary<string, object>? values = null) {
        Card = card;
        OwnerName = ownerName;
        Values = values?.AsReadOnly() ?? new Dictionary<string, object>().AsReadOnly();
    }

    public CardMeta(Card card, string? ownerName, IReadOnlyDictionary<string, object> values) {
        Card = card;
        OwnerName = ownerName;
        Values = values;
    }

    /// <summary>
    /// Creates a card meta from this <see cref="CardMeta"/>.
    /// </summary>
    /// <param name="card">A new card type to use. If no value is provided instead the same <see cref="Card"/> will be used.</param>
    /// <param name="ownerName">The owner of the card</param>
    public CardMeta Derive(Card? card = null, string? ownerName = null) => new(card ?? Card, ownerName, Values);

    /// <summary>
    /// Creates a new card meta from a <see cref="Card"/>
    /// </summary>
    /// <param name="card">The card that will be used.</param>
    /// <param name="ownerName">The owner of the card that will be created. This is not neccasiry.</param>
    /// <param name="token">A <paramref name="token"/> to be used for <see cref="Values"/>.</param>
    /// <returns>The created <see cref="CardMeta"/>.</returns>
    public static CardMeta CreateNew(Card card, string? ownerName = null, IDictionary<string, object>? values = null) => new(card, ownerName, values);

}
