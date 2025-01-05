using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Resources;
using MoDuel.Shared.Json;
using MoDuel.Shared.Structures;

namespace DefaultPackage;

// This file specifically relates to card instance display values.
public static partial class CardActions {

    /// <summary>
    /// Get the level that display actions will use to retrieve their base values.
    /// </summary>
    [ActionName(nameof(GetCardDisplayLevelDefault))]
    public static int GetCardDisplayLevelDefault(CardInstance card) {
        var baseLevel = card.GetLevelDefault();
        var state = card.Context;

        var overwriteTable = new DataTable() {
                { "Card",  card },
                { "Level", baseLevel }
            };

        card.AbilityDataTrigger("Overwrite:CardDisplayStatLevel", ref overwriteTable);

        return overwriteTable.Get<int?>("Level") ?? 1;
    }

    /// <summary>
    /// Get the default level that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedLevelDefault))]
    public static string? GetDisplayedLevelDefault(CardInstance card) {
        int level = GetCardDisplayLevelDefault(card);
        var overwriteTable = new DataTable() {
            { "Card", card },
            { "level", level },
            { "LevelDisplay", level.ToString() },
            { "Unknown", false } // Is the value hidden.
        };
        card.AbilityDataTrigger("Overwrite:GetDisplayedLevel", ref overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string>("LevelDisplay");
    }

    /// <summary>
    /// Get the default attack that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedAttackDefault))]
    public static string? GetDisplayedAttackDefault(CardInstance card) {
        int level = GetCardDisplayLevelDefault(card);
        if (card.Imprint.Type != "Creature")
            return null;
        int attack = card.Imprint.GetLevelledParameter("Attack", level).ToRawValueOrFallback(0);

        var overwriteTable = new DataTable() {
            { "Card", card },
            { "Level", level },
            { "Attack", attack },
            { "AttackDisplay", attack.ToString() },
            { "Unknown", false }
        };

        card.AbilityDataTrigger("Overwrite:GetDisplayedAttack", ref overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string?>("AttackDisplay"); ;
    }


    /// <summary>
    /// Get the default armour that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedArmourDefault))]
    public static string? GetDisplayedArmourDefault(CardInstance card) {
        int level = GetCardDisplayLevelDefault(card);
        if (card.Imprint.Type != "Creature")
            return null;
        int armour = card.Imprint.GetLevelledParameter("Armour", level).ToRawValueOrFallback(0);
        var overwriteTable = new DataTable() {
            { "Card", card },
            { "Level", level },
            { "Armour", armour },
            { "ArmourDisplay", armour.ToString() },
            { "Unknown", false }
        };
        card.AbilityDataTrigger("Overwrite:GetDisplayedArmour", ref overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string>("ArmourDisplay");
    }

    /// <summary>
    /// Get the default life that will be displayed on the card.
    /// </summary>
    [ActionName(nameof(GetDisplayedLifeDefault))]
    public static string? GetDisplayedLifeDefault(CardInstance card) {
        int level = GetCardDisplayLevelDefault(card);
        if (card.Imprint.Type != "Creature")
            return null;
        int life = card.Imprint.GetLevelledParameter("Life", level).ToRawValueOrFallback(1);
        var overwriteTable = new DataTable() {
            { "Card", card },
            { "Level", level },
            { "Life", life },
            { "LifeDisplay", life.ToString() }, 
            { "Unknown", false }
        };
        card.AbilityDataTrigger("Overwrite:GetDisplayedLife", ref overwriteTable);
        bool unknown = overwriteTable.Get<bool>("Unknown");
        return unknown ? null : overwriteTable.Get<string>("LifeDisplay");
    }


    /// <summary>
    /// Get the default cost that will be displayed on the card.
    /// <para>This should be the highest possible cost so that Players are confused why they cant afford the cost.</para>
    /// </summary>
    [ActionName(nameof(GetDisplayedCostDefault))]
    public static ResourceCost? GetDisplayedCostDefault(this CardInstance card) {
        // TODO DELAY: Make resource cost equal real cost.
        int level = GetCardDisplayLevelDefault(card);
        var cost = GetLevelledCost(card.Imprint, level);
        var overwriteTable = new DataTable() {
            { "Card", card },
            { "level", level },
            { "Cost", cost }
        };
        card.AbilityDataTrigger("Overwrite:GetDisplayedCost", ref overwriteTable);
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
