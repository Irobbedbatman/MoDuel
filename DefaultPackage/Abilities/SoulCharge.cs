using MoDuel;
using MoDuel.Abilities;
using MoDuel.Client;
using MoDuel.Heroes;
using MoDuel.Shared.Structures;
using MoDuel.Triggers;

namespace DefaultPackage.Abilities;
public class SoulCharge : Ability {


    public static void Heal(AbilityReference self, Trigger trigger, DataTable data) {

        var player = data["Player"];
        if (self.Holder is HeroInstance hero) {
            if (hero.Owner == player) {
                HealActions.Heal(hero.Owner, 1);
                hero.Owner.Context.SendBlockingRequest(new ClientRequest("LogText", "Abc"), 0);
                Console.WriteLine("Soul Charge Activate!!!");
            }
        }
    }

    public override ActionFunction? CheckTrigger(Trigger trigger) {
        if (trigger.TriggerType == TriggerType.Implicit) {
            if (trigger.Key == "AfterMeditate") {
                return new ActionFunction(Heal);
            }
        }
        return null;
    }

    public override string GetDescription() => "Gain health when mediate.";

    public override string GetName() => "SoulCharge";

    public override object?[] GetParameters(AbilityReference reference) => [];

}
