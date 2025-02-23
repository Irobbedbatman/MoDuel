using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Heroes;
using MoDuel.Resources;
using MoDuel.Serialization;
using MoDuel.Shared.Data;
using System.Collections.Frozen;

namespace MoDuel.Players;

/// <summary>
/// The information that will be used to create a <see cref="Player"/>.
/// <para>This information is from the player choices and permanent data.</para>
/// </summary>
public class PlayerMetaLoaded {

    /// <summary>
    /// The id of the player.
    /// </summary>
    public readonly string UserId;

    /// <summary>
    /// The list of <see cref="ResourceType"/>s that will be used to make a new <see cref="ResourcePool"/>.
    /// </summary>
    public readonly ResourceType[] ResourcePoolTypes;

    /// <summary>
    /// The id of the hero the player will use.
    /// </summary>
    public readonly HeroMetaLoaded Hero;

    /// <summary>
    /// The cards that will initially be in the player's hand.
    /// </summary>
    public readonly CardMetaLoaded[] Hand;

    /// <summary>
    /// The cards that will initially be in the player's grave.
    /// </summary>
    public readonly CardMetaLoaded[] Grave;

    /// <summary>
    /// The cards that will initially be on the player's field.
    /// </summary>
    public readonly IReadOnlyDictionary<int, CardMetaLoaded> Field;

    /// <summary>
    /// Assorted data the player may have that is not necessary for the creation of the player.
    /// </summary>
    public readonly FrozenDictionary<string, object?> Values;

    /// <summary>
    /// Create a loaded player meta from it's raw equivalent.
    /// </summary>
    /// <param name="raw">The raw data to read from.</param>
    /// <param name="data">The <see cref="PackageCatalogue"/> to load the card from.</param>
    /// <returns>The created <see cref="PlayerMetaLoaded"/> or null if there was an issue during loading resources.</returns>
    public static PlayerMetaLoaded? CreateFromRawData(PlayerMeta raw, PackageCatalogue data) {
        
        var hero = HeroMetaLoaded.CreateFromRawData(raw.Hero, data);
        if (hero == null) return null;

        List<ResourceType> resources = [];
        foreach (var resource in raw.ResourceTypes) {     
            var type = data.LoadResourceType(resource);
            if (type == null) return null;
        }

        var hand = CardMetaLoaded.LoadCardSet(raw.HandCards, data, raw.PlayerId);
        if (hand == null) return null;
        var grave = CardMetaLoaded.LoadCardSet(raw.GraveCards, data, raw.PlayerId);
        if (grave == null) return null;
        var field = CardMetaLoaded.LoadCardSetPositional(raw.FieldCards, data, raw.PlayerId);
        if (field == null) return null;

        // Build the meta values from the data.
        var values = new Dictionary<string, object?>();
        foreach (var value in raw.Values) {
            values.Add(value.Key, value.Value);
        }

        return new PlayerMetaLoaded(raw.PlayerId, [.. resources], hero, hand, grave, field, values);
    }


    private PlayerMetaLoaded(string userId, ResourceType[] resourcePoolTypes, HeroMetaLoaded hero, IEnumerable<CardMetaLoaded> hand, IEnumerable<CardMetaLoaded> grave, IDictionary<int, CardMetaLoaded> field, Dictionary<string, object?> values) {
        UserId = userId;
        ResourcePoolTypes = resourcePoolTypes;
        Hero = hero;
        Hand = hand.ToArray();
        Grave = grave.ToArray();
        Field = field.AsReadOnly();
        Values = values.ToFrozenDictionary();
    }
}
