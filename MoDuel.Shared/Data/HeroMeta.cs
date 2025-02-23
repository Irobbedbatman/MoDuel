using System.Text.Json.Nodes;

namespace MoDuel.Shared.Data;

/// <summary>
/// Information used to create a hero instance.
/// </summary>
public class HeroMeta {

    /// <summary>
    /// The id of the hero to load.
    /// </summary>
    public readonly string HeroId;

    /// <summary>
    /// Unique information passed to the hero instance.
    /// </summary>
    public readonly JsonObject Values = [];

    public HeroMeta(string heroId, JsonObject values) {
        HeroId = heroId;
        Values = values;
    }

}
