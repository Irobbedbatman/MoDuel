using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Resources;
using MoDuel.Serialization;

namespace MoDuel.Players;

/// <summary>
/// The information that will be used to create a <see cref="Player"/>.
/// <para>This information is from the player choices and permanent data.</para>
/// </summary>
[SerializeReference]
public class PlayerMeta(string userId, ResourceType[] resourcePoolTypes, Hero hero, IEnumerable<CardMeta> hand, IEnumerable<CardMeta> grave, IDictionary<int, CardMeta> field, Dictionary<string, object> values) {

    /// <summary>
    /// The id of the player.
    /// </summary>
    public readonly string UserId = userId;

    /// <summary>
    /// The list of <see cref="ResourceType"/>s that will be used to make a new <see cref="ResourcePool"/>.
    /// </summary>
    public readonly ResourceType[] ResourcePoolTypes = resourcePoolTypes;

    /// <summary>
    /// The id of the hero the player will use.
    /// </summary>
    public readonly Hero Hero = hero;

    /// <summary>
    /// The cards that will initially be in the player's hand.
    /// </summary>
    public readonly CardMeta[] Hand = hand.ToArray();

    /// <summary>
    /// The cards that will initially be in the player's grave.
    /// </summary>
    public readonly CardMeta[] Grave = grave.ToArray();

    /// <summary>
    /// The cards that will initially be on the player's field.
    /// </summary>
    public readonly IReadOnlyDictionary<int, CardMeta> Field = field.AsReadOnly();

    /// <summary>
    /// Assorted data the player may have that is not necessary for the creation of the player.
    /// </summary>
    public readonly IReadOnlyDictionary<string, object> Values = values.AsReadOnly();
}
