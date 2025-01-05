using MoDuel;
using MoDuel.Cards;
using MoDuel.Effects;
using MoDuel.Sources;

namespace DefaultPackage.Effects;
public class EffectGainAttack : Effect {


    public EffectGainAttack(Source source, Target target) : base(source, target) {

    }

    public override void Apply() {
        if (Target is CardInstance card) {
            card.Attack++;
        }
    }

    public override string GetDescription() {
        return "Provided attack boost of [<0>].";
    }

    public override string GetName() {
        return "Attack Boost";
    }

    public override object?[] GetParameters() {
        return [1];
    }

    public override void Remove() {
        if (Target is CardInstance card) {
            card.Attack--;
        }
    }
}
