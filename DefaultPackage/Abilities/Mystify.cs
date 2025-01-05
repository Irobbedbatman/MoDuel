using MoDuel;
using MoDuel.Abilities;
using MoDuel.Shared.Structures;
using MoDuel.Triggers;

namespace DefaultPackage.Abilities;

public class Mystify : Ability {

    public static void HideAttack(AbilityReference self, Trigger trigger, DataTable data) {

        // ensure the trigger is from a card.

        data["Unknown"] = true;
    }

    public override ActionFunction? CheckTrigger(Trigger trigger) {

        if (trigger.TriggerType == TriggerType.DataOverride) {

            if (trigger.Key == "GetDisplayedAttack") {
                return new ActionFunction(HideAttack);
            }
        }

        return null;
    }

    public override string GetDescription() => "Desc: " + GetName();

    public override string GetName() => "Mystify";

    public override object?[] GetParameters(AbilityReference reference) {
        return [];
    }
}
