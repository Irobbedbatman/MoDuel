using MoDuel;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.Triggers;

namespace DefaultPackage.CardAbilities;

/// <summary>
/// The set that contains card abilities.
/// </summary>
public static class CardAbiltitiesSet {

    /// <summary>
    /// Mysticfy ability.
    /// <para>Makes the display that is overwrtitten hidden.</para>
    /// </summary>
    [ActionName(nameof(Mysticfy))]
    public static void Mysticfy(CardInstance card, OverwriteTable table) {
        if (card.IsAlive)
            table["Unknown"] = true;
    }

    /// <summary>
    /// Calls <see cref="Mystic1(CardInstance)"/> and <see cref="Mystic2(CardInstance)"/> in a cycle.
    /// </summary>
    [ActionName(nameof(MysticX))]
    public static void MysticX(CardInstance card, Player _, ResourceType _1, int _2) {

        var ability = card.Values.GetValueOrDefault("Abil", null);
        if (ability == null) {
            ability = card.Context.PackageCatalogue.LoadAction(nameof(Mystic1), card.Imprint.Package);
            card.Values["Abil"] = ability;
        }
        ((ActionFunction)ability).Call(card);

    }

    /// <summary>
    /// Makes the cards attack + 1 and the updates <see cref="MysticX(CardInstance, Player, ResourceType, int)"/>
    /// </summary>
    [ActionName(nameof(Mystic1))]
    public static void Mystic1(CardInstance card) {

        if (card is CreatureInstance creature) {
            creature.Attack += 1;
        }

        var ability = card.Context.PackageCatalogue.LoadAction(nameof(Mystic2), card.Imprint.Package);
        card.Values["Abil"] = ability;
    }

    /// <summary>
    /// Makes the cards Max/Life + 1 and the updates <see cref="MysticX(CardInstance, Player, ResourceType, int)"/>
    /// </summary>

    [ActionName(nameof(Mystic2))]
    public static void Mystic2(CardInstance card) {

        if (card is CreatureInstance creature) {
            creature.Life += 1;
            creature.MaxLife += 1;
        }

        var ability = card.Context.PackageCatalogue.LoadAction(nameof(Mystic1), card.Imprint.Package);
        card.Values["Abil"] = ability;
    }
}
