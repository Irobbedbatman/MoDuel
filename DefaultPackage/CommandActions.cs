using DefaultPackage.ContentLookups;
using MoDuel;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.State;
using MoDuel.Triggers;

namespace DefaultPackage;

/// <summary>
/// The actions that are used as the generic commands and components those commands rely upon.
/// </summary>
public static class CommandActions {

    /// <summary>
    /// The cost a command normal would take. This is one action point.
    /// </summary>
    public static ResourceCost NormalCommandCost => new() {
        new ResourceCounter(ResourceTypes.ActionPoints, 1)
    };

    /// <summary>
    /// The action ran after each command to reset command information for the next command.
    /// </summary>=
    public static void EndCommand(DuelState state) {
        DamageActions.ResetCreatureKilledFlag(state);
    }

    /// <summary>
    /// The action that performs the meditate command.
    /// </summary>
    [ActionName(nameof(CmdMeditate))]
    [Dependency(DependencyAttribute.DependencyTypes.ResourceType, "ActionPoints")]
    public static bool CmdMeditate(Player player) {
        if (!player.IsTurnOwner()) return false;
        if (ResourceActions.PayCost(player, NormalCommandCost)) {
            ResourceActions.Meditate(player);
            EndCommand(player.Context);
            return true;
        }
        return false;
    }

    /// <summary>
    /// The action that performs the discard command.
    /// </summary>
    /// <param name="cardIndex">The target index of a card recorded in the target registry.</param>
    [ActionName(nameof(CmdDiscard))]
    public static bool CmdDiscard(Player player, int cardIndex) {
        if (!player.IsTurnOwner()) return false;

        DuelState state = player.Context;
        TargetRegistry targets = state.TargetRegistry;

        var target = targets.GetTarget<Target>(cardIndex);

        // Ensure the card is in the hand.
        if (target is not CardInstance card || !card.InHand || card.Owner != player) {
            return false;
        }

        if (ResourceActions.PayCost(player, NormalCommandCost)) {
            player.Discard(card);
            EndCommand(state);
            return true;
        }
        return false;
    }

    /// <summary>
    /// The action that performs the level up command.
    /// </summary>
    [ActionName(nameof(CmdLevelUp))]
    public static bool CmdLevelUp(Player player) {
        if (!player.IsTurnOwner()) return false;

        if (player.CanLevelUp() && ResourceActions.PayCost(player, NormalCommandCost)) {
            ExpAndLevelingActions.LevelUp(player);
            EndCommand(player.Context);
            return true;
        }
        return false;
    }

    /// <summary>
    /// The action that performs the revive command.
    /// </summary>
    [ActionName(nameof(CmdRevive))]
    public static bool CmdRevive(Player player) {
        if (!player.IsTurnOwner()) return false;

        if (player.Grave.Count > 0 && ResourceActions.PayCost(player, NormalCommandCost)) {
            player.ReturnDeadCardsToHand();
            EndCommand(player.Context);
            return true;
        }
        return false;
    }

    /// <summary>
    /// The action that performs the end of turn command and moves to the combat phase.
    /// </summary>
    [ActionName(nameof(CmdEndTurn))]
    public static bool CmdEndTurn(Player player) {
        if (!player.IsTurnOwner())
            return false;
        CombatActions.ExecuteCombatPhase(player.Context);
        TurnActions.ChangeTurn(player.Context);
        return true;
    }

    /// <summary>
    /// The action that performs the card payed command.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cardIndex">The target index of the card to be played that is in the target registry.</param>
    /// <param name="targetIndex">The target index of the card;s target that is in the target registry.</param>
    [ActionName(nameof(CmdPlayCard))]
    public static bool CmdPlayCard(Player player, int cardIndex, int targetIndex) {
        if (!player.IsTurnOwner()) return false;

        DuelState state = player.Context;
        TargetRegistry targets = state.TargetRegistry;

        CardInstance? card = targets.GetTarget<CardInstance>(cardIndex);
        Target? target = targets.GetTarget<Target>(targetIndex);

        if (card == null || target == null) return false;

        var overwrite = new OverwriteTable() {
            { "Card", card },
            { "Target", target },
            { "Player", player }
        };

        state.OverwriteTrigger("CardPlayedOverwrite", overwrite);

        if (card == null || target == null || !card.InHand) return false;

        card = overwrite.Get<CardInstance>("Card");
        target = overwrite.Get<Target>("Target");

        card?.FallbackTrigger("PlayCard", DefaultPlayActions.PlayCard, target);
        card?.RemoveFromHand();

        // TODO DELAY: Return false if the card could not be played.

        state.Trigger("CardPlayed", card, target);

        EndCommand(state);

        return true;

    }


}
