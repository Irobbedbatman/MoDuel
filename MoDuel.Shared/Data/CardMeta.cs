using System.Text.Json.Nodes;

namespace MoDuel.Shared.Data;

/// <summary>
/// Information used to create a card instance.
/// </summary>
public class CardMeta {

    /// <summary>
    /// The id of the card to load.
    /// </summary>
    public readonly string CardId;

    /// <summary>
    /// Unique information to provide to the instance of the card.
    /// </summary>
    public readonly JsonObject Values = [];

    public CardMeta(string cardId, JsonObject values) {
        CardId = cardId;
        Values = values;
    }

}
