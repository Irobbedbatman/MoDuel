using MoDuel.Data;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// The package that contains the default game logic.
/// </summary>
[PackagedCodeBinding("DefaultPackage")]
public class DefaultPackage : PackagedCode {

    public const string Name = "Default";

    public DefaultPackage(Package sourcePackage) : base(sourcePackage) { }

    public override ICollection<Delegate> GetAllActions() {

        // There are two ways to get all the actions.

        // The first is directly link them all.
        var list = new List<Delegate>() {
            GlobalActions.Fizzle
        };

        // The second is to a automatde system to get all actions with the ActioName attribute.
        return GetAllActionsViaTag();


    }

    public override void OnDuelLoaded(DuelState state) {

    }

    public override void OnPackageLoaded(Package package) {

    }
}
