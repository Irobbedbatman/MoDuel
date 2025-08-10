using MoDuel.Shared.Json;
using System.Text.Json.Nodes;

namespace MoDuel.Shared.Data;

/// <summary>
/// Information used to create a card instance.
/// </summary>
public class CardMeta {

    /// <summary>
    /// The id of the card to load.
    /// </summary>
    public string CardId => Values.Get("CardId").ToRawValue<string>() ?? "";

    /// <summary>
    /// The style that determines how the card is displayed.
    /// </summary>
    public string Style => Values.Get("Style").ToRawValue<string>() ?? "Default";

    /// <summary>
    /// Unique information to provide to the instance of the card.
    /// </summary>
    public readonly JsonObject Values = [];

    public CardMeta(JsonObject values) {
        Values = values;
    }

}
