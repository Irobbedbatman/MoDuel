using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Resources;
using MoDuel.Serialization;

namespace MoDuel.Players;

/// <summary>
/// The information that will be used to create a <see cref="Player"/>.
/// <para>This information is from the player choices and permanant data.</para>
/// </summary>
[SerializeReference]
public class PlayerMeta {

    /// <summary>
    /// The id of the player.
    /// </summary>
    public readonly string UserId;

    /// <summary>
    /// The list of <see cref="ResourceType"/>s that will be used to make a new <see cref="ResourcePool"/>.
    /// </summary>
    public readonly ResourceType[] ManaPool;

    /// <summary>
    /// The id of the hero the player will use.
    /// </summary>
    public readonly Hero Hero;

    /// <summary>
    /// The cards that will initially be in the player's hand.
    /// </summary>
    public readonly CardMeta[] Hand;

    /// <summary>
    /// The cards that will initially be in the player's grave.
    /// </summary>
    public readonly CardMeta[] Grave;

    /// <summary>
    /// The cards that will initially be on the player's field.
    /// </summary>
    public readonly IReadOnlyDictionary<int, CardMeta> Field;

    /// <summary>
    /// Assorted data the player may have that is not neccassary for the creation of the player.
    /// </summary>
    public readonly IReadOnlyDictionary<string, object> Values;

    public PlayerMeta(string userId, ResourceType[] manapool, Hero hero, IEnumerable<CardMeta> hand, IEnumerable<CardMeta> grave, IDictionary<int, CardMeta> field, Dictionary<string, object> values) {
        UserId = userId;
        ManaPool = manapool;
        Hero = hero;
        Hand = hand.ToArray();
        Grave = grave.ToArray();
        Field = field.AsReadOnly();
        Values = values.AsReadOnly();
    }

}
