using MoDuel.Data;
using MoDuel.Shared.Data;
using System.Collections.Frozen;

namespace MoDuel.Cards;

/// <summary>
/// A set of values used to define special values for <see cref="CardInstance"/>s.
/// </summary>
public class CardMetaLoaded {

    /// <summary>
    /// The player that owns the card. If null the card instance has no original owner.
    /// </summary>
    public readonly string? OwnerName;
    /// <summary>
    /// The loaded card data that defines what the meta used in.
    /// </summary>
    public readonly Card Card;
    /// <summary>
    /// The raw meta data values.
    /// </summary>
    public readonly FrozenDictionary<string, object?> Values;

    /// <summary>
    /// Create a loaded card meta from it's raw equivalent.
    /// </summary>
    /// <param name="raw">The raw data to read from.</param>
    /// <param name="data">The <see cref="PackageCatalogue"/> to load the card from.</param>
    /// <param name="ownerName">The owner of the card that will be created. This is not necessary.</param>
    /// <returns>The created <see cref="CardMetaLoaded"/> or null if there was an issue during loading resources.</returns>
    public static CardMetaLoaded? CreateFromRawData(CardMeta raw, PackageCatalogue data, string? ownerId = null) {
        
        // Build the meta values from the data.
        var values = new Dictionary<string, object?>();
        foreach (var value in raw.Values) {
            values.Add(value.Key, value.Value);
        }

        // Attempt to load the card, return null if the card does not exist.
        var card = data.LoadCard(raw.CardId);
        if (card == null) return null;

        return new CardMetaLoaded(card, ownerId, values.ToFrozenDictionary());
    }

    /// <summary>
    /// A helper to load a set of cards such as the hand or grave.
    /// </summary>
    /// <param name="cards">the raw data to load.</param>
    /// <param name="data">The <see cref="PackageCatalogue"/> to load the card from.</param>
    /// <param name="ownerName">The owner of the card that will be created. This is not necessary.</param>
    /// <returns>The set of <see cref="CardMetaLoaded"/>s created or null if there was an issue during loading resources.</returns>
    public static CardMetaLoaded[]? LoadCardSet(CardMeta[] cards, PackageCatalogue data, string? ownerId = null) {
        CardMetaLoaded[] result = new CardMetaLoaded[cards.Length];
        for (int i = 0; i < cards.Length; i++) {
            var card = CreateFromRawData(cards[i], data, ownerId);
            if (card == null) return null;
            result[i] = card;
        }
        return result;
    }

    /// <summary>
    /// A helper to load a set of cards with positional indices such as the field.
    /// </summary>
    /// <param name="cards">the raw data to load.</param>
    /// <param name="data">The <see cref="PackageCatalogue"/> to load the card from.</param>
    /// <param name="ownerName">The owner of the card that will be created. This is not necessary.</param>
    /// <returns>The set of <see cref="CardMetaLoaded"/>s indexed by their position created or null if there was an issue during loading resources.</returns>
    public static Dictionary<int, CardMetaLoaded>? LoadCardSetPositional(Dictionary<int, CardMeta> cards, PackageCatalogue data, string? ownerId = null) {
        Dictionary<int, CardMetaLoaded> result = [];
        foreach (var pair in cards) {
            var card = CreateFromRawData(pair.Value, data, ownerId);
            if (card == null) return null;
            result.Add(pair.Key, card);
        }
        return result;
    }


    /// <summary>
    /// Create a clone of the meta.
    /// </summary>
    /// <param name="sameOwner">Whether the meta should use the same owner.</param>
    /// <param name="newOwner">The owner used if <paramref name="sameOwner"/> is false.</param>
    /// <returns>The newly created card meta.</returns>
    public CardMetaLoaded Clone(bool sameOwner, string? newOwner = null) => new(Card, sameOwner ? OwnerName : newOwner, Values);


    /// <summary>
    /// Creates a card meta from this <see cref="CardMetaLoaded"/>.
    /// </summary>
    /// <param name="card">A new card type to use. If no value is provided instead the same <see cref="Card"/> will be used.</param>
    /// <param name="ownerName">The owner of the card</param>
    public CardMetaLoaded Derive(Card? card = null, string? ownerName = null) => new(card ?? Card, ownerName, Values);

    /// <summary>
    /// Creates a new card meta from a <see cref="Card"/>
    /// </summary>
    /// <param name="card">The card that will be used.</param>
    /// <param name="ownerName">The owner of the card that will be created. This is not necessary.</param>
    /// <param name="values">A set of values to be used for <see cref="Values"/>.</param>
    /// <returns>The created <see cref="CardMetaLoaded"/>.</returns>
    public static CardMetaLoaded CreateNew(Card card, string? ownerName = null, IDictionary<string, object?>? values = null) => new(card, ownerName, (values ?? new Dictionary<string, object?>()).ToFrozenDictionary());

    private CardMetaLoaded(Card card, string? ownerName, FrozenDictionary<string, object?> values) {
        Card = card;
        OwnerName = ownerName;
        Values = values;
    }

}
