using System.Text.Json.Nodes;

namespace MoDuel.Shared.Data;

/// <summary>
/// Information used to create the initial state of a player instance.
/// </summary>
public class PlayerMeta {

    /// <summary>
    /// The unique id of this player.
    /// </summary>
    public PlayerId Id;

    /// <summary>
    /// The id of the player.
    /// </summary>
    public string Name;

    /// <summary>
    /// The initial hero of the player.
    /// </summary>
    public HeroMeta Hero;

    /// <summary>
    /// The initial resource types in the player's resource pool.
    /// </summary>
    public string[] ResourceTypes;

    /// <summary>
    /// The data of the cards in the player's hand initially.
    /// </summary>
    public CardMeta[] HandCards { init; get; }

    /// <summary>
    /// The data of the cards on the player's field initially. Mapped by the slot index.
    /// </summary>
    public Dictionary<int, CardMeta> FieldCards { init; get; }

    /// <summary>
    /// The data of the cards in the player's grave initially.
    /// </summary>
    public CardMeta[] GraveCards { init; get; }

    /// <summary>
    /// Unique information to the pass to the player from outside of the duel.
    /// </summary>
    public JsonObject Values { init; get; }

    public PlayerMeta(PlayerId id, string name, HeroMeta hero, string[] resourceTypes, JsonObject values) {
        Id = id;
        Name = name;
        Hero = hero;
        ResourceTypes = resourceTypes;
        HandCards = [];
        GraveCards = [];
        FieldCards = [];
        Values = values;
    }

}
