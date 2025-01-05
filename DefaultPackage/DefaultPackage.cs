using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.State;
using MoDuel.Tools;

namespace DefaultPackage;

/// <summary>
/// The package that contains the default game logic.
/// </summary>
[PackagedCodeBinding("DefaultPackage")]
public class DefaultPackage : PackagedCode {

#nullable disable
    /// <summary>
    /// Get the local instance of the package per thread.
    /// <para>Only returns a value when the duel is started.</para>
    /// </summary>
    public static Package GetPackage() => ThreadContext.GetPackageInstance(Name);
#nullable enable


    public const string Name = "Default";

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

    public override void OnDuelLoaded(DuelState state) {

    }

    public override void OnPackageLoaded(Package package) {

    }
}
