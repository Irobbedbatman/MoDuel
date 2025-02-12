using MoDuel.Data.Assembled;
using MoDuel.Data;
using MoDuel.State;

[assembly: PackageAssembly(typeof(DefaultPackage.DefaultPackage))]

namespace DefaultPackage;

/// <summary>
/// The package that contains the default game logic.
/// </summary>
public class DefaultPackage : PackagedCode {

    public static DefaultPackage Instance => GetInstanceViaAssembly<DefaultPackage>();

    public static string Name => Instance.Package.Name;

    public DefaultPackage(Package sourcePackage) : base(sourcePackage) { }

    public override ICollection<Delegate> GetAllActions() {

        // There are two ways to get all the actions.

        // The first is directly link them all.
        var list = new List<Delegate>() {
            GlobalActions.Fizzle
        };

        // The second is a an automated system to get all actions with the ActionName attribute.
        return GetAllActionsViaTag();


    }

    public override void OnDuelLoaded(DuelState state) { }

    public override void OnPackageLoaded(Package package) { }

}
