using MoDuel.Cards;
using MoDuel.Data.Assembled;
using MoDuel.Resources;
using MoDuel.Shared.Json;

namespace DefaultPackage;

// Actions to the stats of a card.
public static partial class CardActions {

    /// <summary>
    /// Get the default level of a card before being summoned.
    /// </summary>
    [ActionName(nameof(GetLevelDefault))]
    public static int GetLevelDefault(this CardInstance card) => card.FixedLevel ?? card.Owner?.Level ?? 1;

    /// <summary>
    /// Get the default attack of a card before being summoned.
    /// </summary>
    [ActionName(nameof(GetAttackDefault))]
    public static int GetAttackDefault(CardInstance card, int level) => card.Imprint.GetLevelledParameter("Attack", level).ToRawValueOrFallback(0);


    /// <summary>
    /// Get the default armour of a card before it is summoned.  
    /// </summary>
    [ActionName(nameof(GetArmourDefault))]
    public static int GetArmourDefault(CardInstance card, int level) => card.Imprint.GetLevelledParameter("Armour", level).ToRawValueOrFallback(0);


    /// <summary>
    /// Get the max life of a card before it is summoned.
    /// </summary>
    [ActionName(nameof(GetMaxLifeDefault))]
    public static int GetMaxLifeDefault(CardInstance card, int level) => card.Imprint.GetLevelledParameter("Life", level).ToRawValueOrFallback(1);

    /// <summary>
    /// Get the cost of a card.
    /// </summary>
    [ActionName(nameof(GetCostDefault))]
    public static ResourceCost GetCostDefault(CardInstance card, int level) => card.Imprint.GetLevelledCost(level);

}
