using DefaultPackage.DataTables;
using MoDuel;
using MoDuel.Abilities;
using MoDuel.Client;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.Triggers;

namespace DefaultPackage.Abilities;
public class ActionPointBehaviour : Ability {

    public static void CanPayCost(AbilityReference self, Trigger trigger, CostCounterDataTable data) {

        var player = data.Player;
        var cost = data.Cost;

        // If the data was has been emptied consider the cost payable.
        if (player == null || cost == null) {
            data["Result"] = true;
            return;
        }

        // Check if the player has enough action points.
        var actionPoints = cost.Amount;
        data["Result"] = player.ActionPoints >= actionPoints;
    }

    public static void PayCost(Player player, ResourceCounter cost) {

        int actionPoints = cost.Amount;
        player.ActionPoints -= actionPoints;

        var data = new MoDuel.Shared.Structures.DataTable() {
            ["Player"] = player,
            ["Amount"] = cost.Amount
        };

        // Send a trigger that the actions points have been spent.
        var state = player.Context;
        state.SendRequest(new ClientRequest("ActionPointsPayed", actionPoints));
        var payedTrigger = new Trigger("ActionPointsPayed", new MoDuel.Sources.Source(), state, TriggerType.Implicit);
        state.Trigger(payedTrigger, data);

        // Players gain exp when they spend action points.
        ExpAndLevellingActions.GainExp(player, 2);
    }

    public static void GetPayCost(AbilityReference _, Trigger t_, CostCounterDataTable data) {
        data["PayAction"] = new ActionFunction(PayCost);
    }

    public override ActionFunction? CheckTrigger(Trigger trigger) {
        if (trigger.TriggerType == TriggerType.ExplicitData) {
            switch (trigger.Key) {
                case "CanPay":
                    return new ActionFunction(CanPayCost);
                case "GetPayAction":
                    return new ActionFunction(GetPayCost);
            }
        }
        return null;
    }

    public override string GetDescription() => "The behaviour that handles action points.";

    public override string GetName() => "Action Point Behaviour";

    public override object?[] GetParameters(AbilityReference reference) => [];

}
