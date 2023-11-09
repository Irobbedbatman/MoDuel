using MoDuel.Client;
using MoDuel.Resources;

namespace MoDuel.Cards;

// See CardInstance.cs for documentation.
public partial class CardInstance {

    /// <summary>
    /// One of the current stats of this card.
    /// </summary>
    public int Level, Life, Attack, Armour, MaxLife;

    /// <summary>
    /// Get the level that this card should display. Cards on the field have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedLevel() {
        return FallbackTrigger(nameof(GetDisplayedLevel), Context.Settings.CardGetDisplayedLevelFallback);
    }

    /// <summary>
    /// Get the cost that this card should display. Cards on the field have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public ResourceCost? GetDisplayedCost() {
        return FallbackTrigger(nameof(GetDisplayedCost), Context.Settings.CardGetDisplayedCostFallback);
    }

    /// <summary>
    /// Get the attack that this card should display. Cards on the field have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedAttack() {
        return FallbackTrigger(nameof(GetDisplayedAttack), Context.Settings.CardGetDisplayedAttackFallback);
    }


    /// <summary>
    /// Get the armour that this card should display. Cards on the field have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedArmour() {
        return FallbackTrigger(nameof(GetDisplayedArmour), Context.Settings.CardGetDisplayedArmourFallback);
    }

    /// <summary>
    /// Get the life that this card should display. Cards on the field have a fixed value.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// </summary>
    public string? GetDisplayedLife() {
        return FallbackTrigger(nameof(GetDisplayedLife), Context.Settings.CardGetDisplayedLifeFallback);
    }

    /// <summary>
    /// Get the description that this card should display.
    /// <para>This calls a fallback trigger of the same name.</para>
    /// TOOD: Determine what a description should be.
    /// </summary>
    public ParametrizedBlock[] GetDisplayedDescription() {
        return FallbackTrigger(nameof(GetDisplayedDescription), Context.Settings.CardGetDisplayedDescriptionFallback) ?? Array.Empty<ParametrizedBlock>();
    }


}
