using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Json;
using MoDuel.Resources;
using MoDuel.Tools;
using MoDuel.Triggers;

namespace DefaultPackage;

// Actions to the stats of a card.
public static partial class CardActions {

    /// <summary>
    /// Get the default level of a card before being summoned.
    /// </summary>
    [ActionName(nameof(GetLevelDefault))]
    public static int GetLevelDefault(this CardInstance card) {
        var baseLevel = card.FixedLevel ?? card.Owner?.Level ?? 1;
        var state = card.Context;

        var overwriteTable = new OverwriteTable() {
            { "Card",  card },
            { "Level", baseLevel }
        };

        state.OverwriteTrigger("CardLevelOverwrite", overwriteTable);

        return overwriteTable.Get<int>("Level");
    }

    /// <summary>
    /// Get the default attack of a card before being summoned.
    /// </summary>
    [ActionName(nameof(GetAttackDefault))]
    public static int GetAttackDefault(CardInstance card, int level) {

        int attack = card.Imprint.GetLeveledParameter("Attack", level).ToRawValueOrFallback(0);

        var overwriteTable = new OverwriteTable() {
            { "State", ThreadContext.DuelState },
            { "Card", card },
            { "Level", level},
            { "Attack", attack }
        };

        card.Context.OverwriteTrigger("CardAttackOverwrite", overwriteTable);

        return overwriteTable.Get<int>("Attack");

    }

    /// <summary>
    /// Get the default armour of a card before it is summoned.  
    /// </summary>
    [ActionName(nameof(GetArmourDefault))]
    public static int GetArmourDefault(CardInstance card, int level) {

        int armour = card.Imprint.GetLeveledParameter("Armour", level).ToRawValueOrFallback(0);

        var overwriteTable = new OverwriteTable() {
            { "State", ThreadContext.DuelState },
            { "Card", card },
            { "Level", level},
            { "Armour", armour }
        };

        card.Context.OverwriteTrigger("CardArmourOverwrite", overwriteTable);

        return overwriteTable.Get<int>("Armour");

    }

    /// <summary>
    /// Get the max life of a card before it is summoned.
    /// </summary>
    [ActionName(nameof(GetMaxLifeDefault))]
    public static int GetMaxLifeDefault(CardInstance card, int level) {
        int life = card.Imprint.GetLeveledParameter("Life", level).ToRawValueOrFallback(1);

        var overwriteTable = new OverwriteTable() {
            { "State", ThreadContext.DuelState },
            { "Card", card },
            { "Level", level},
            { "Life", life }
        };

        card.Context.OverwriteTrigger("CardMaxLifeOverwrite", overwriteTable);
        life = overwriteTable.Get<int>("Life");
        if (life < 1)
            life = 1;
        return life;
    }

    /// <summary>
    /// Get the cost of a card.
    /// </summary>
    [ActionName(nameof(GetCostDefault))]
    public static ResourceCost GetCostDefault(CardInstance card, int level) {
        var cost = card.Imprint.GetLeveledCost(level);
        var overwriteTable = new OverwriteTable() {
            { "State", ThreadContext.DuelState },
            { "Card", card },
            { "Level", level},
            { "Cost", cost }
        };

        card.Context.OverwriteTrigger("CardCostOverwrite", overwriteTable);

        return overwriteTable.Get<ResourceCost>("Cost") ?? [];
    }


}
