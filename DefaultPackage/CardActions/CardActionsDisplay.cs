using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Json;
using MoDuel.Resources;
using MoDuel.Triggers;

namespace DefaultPackage;

// This file specificly relates to card instance display values.
public static partial class CardActions {

    /// <summary>
    /// Get the level that display actions will use to retirve their base values.
    /// </summary>
    [ActionName(nameof(GetCardDisplayLevelDefault))]
    public static int GetCardDisplayLevelDefault(CardInstance card) {
        var baseLevel = card.FixedLevel ?? card.Owner?.Level ?? 1;
        var state = card.Context;

        var overwriteTable = new OverwriteTable() {
                { "Card",  card },
                { "Level", baseLevel }
            };

        state.OverwriteTrigger("Overwrite:CardDisplayStatLevel", overwriteTable);

        return overwriteTable.Get<int?>("Level") ?? 1;
    }

    /// <summary>
    /// Get the level that will the display method will use as a level.
    /// </summary>
    private static int GetDisplayLevelOrFallback(CardInstance card) {
        return card.FallbackTrigger("GetFixedDisplayLevel", new MoDuel.ActionFunction(GetCardDisplayLevelDefault));
    }

    /// <summary>
    /// Get the default level that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedLevelDefault))]
    public static string? GetDisplayedLevelDefault(CardInstance card) {
        int level = GetDisplayLevelOrFallback(card);
        var overwriteTable = new OverwriteTable() {
            { "Card", card },
            { "level", level },
            { "LevelDisplay", level.ToString() },
            { "Unknown", false } // Is the value hidden.
        };
        card.Context.OverwriteTrigger("Overwrite:GetDisplayedLevel", overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string>("LevelDisplay");
    }

    /// <summary>
    /// Get the default attack that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedAttackDefault))]
    public static string? GetDisplayedAttackDefault(CardInstance card) {
        int level = GetDisplayLevelOrFallback(card);
        if (card.Imprint.Type != "Creature")
            return null;
        int attack = card.Imprint.GetLeveledParameter("Attack", level).ToRawValueOrFallback(0);

        var overwriteTable = new OverwriteTable() {
            { "Card", card },
            { "Level", level },
            { "Attack", attack },
            { "AttackDisplay", attack.ToString() },
            { "Unknown", false }
        };

        card.Context.OverwriteTrigger("Overwrite:GetDisplayedAttack", overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string?>("AttackDisplay"); ;
    }


    /// <summary>
    /// Get the default armour that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedArmourDefault))]
    public static string? GetDisplayedArmourDefault(CardInstance card) {
        int level = GetDisplayLevelOrFallback(card);
        if (card.Imprint.Type != "Creature")
            return null;
        int armour = card.Imprint.GetLeveledParameter("Armour", level).ToRawValueOrFallback(0);
        var overwriteTable = new OverwriteTable() {
            { "Card", card },
            { "Level", level },
            { "Armour", armour },
            { "ArmourDisplay", armour.ToString() },
            { "Unknown", false }
        };
        card.Context.OverwriteTrigger("Overwrite:GetDisplayedArmour", overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string>("ArmourDisplay");
    }

    /// <summary>
    /// Get the default life that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedLifeDefault))]
    public static string? GetDisplayedLifeDefault(CardInstance card) {
        int level = GetDisplayLevelOrFallback(card);
        if (card.Imprint.Type != "Creature")
            return null;
        int life = card.Imprint.GetLeveledParameter("Life", level).ToRawValueOrFallback(1);
        var overwriteTable = new OverwriteTable() {
            { "Card", card },
            { "Level", level },
            { "Life", life },
            { "LifeDisplay", life.ToString() },
            { "Unknown", false }
        };
        card.Context.OverwriteTrigger("Overwrite:GetDisplayedLife", overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string>("LifeDisplay");
    }


    /// <summary>
    /// Get the default cost that will be displayed on the card.
    /// <para>This should be the highest possible cost so that Players are confused why they cant afford the cost.</para>
    /// </summary>
    [ActionName(nameof(GetDisplayedCostDefault))]
    public static ResourceCost? GetDisplayedCostDefault(this CardInstance card) {
        // TODO DELAY: Make resouce cost equal real cost.
        int level = GetDisplayLevelOrFallback(card);
        var cost = GetLeveledCost(card.Imprint, level);
        var overwriteTable = new OverwriteTable() {
            { "Card", card },
            { "level", level },
            { "Cost", cost }
        };
        card.Context.OverwriteTrigger("Overwrite:GetDisplayedCost", overwriteTable);
        return overwriteTable.Get<ResourceCost?>("Cost");
    }

    /// <summary>
    /// Get the default description blocks that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedDescriptionDefault))]
    public static ParametrizedBlock[] GetDisplayedDescriptionDefault(CardInstance card) {
        // TODO DELAY: card descriptions.
        return [
            new ParametrizedBlock("CARD_DESC_0000001", card.Imprint.Name)
        ];
    }

}
