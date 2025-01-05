using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Players;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// lThe category for actions that to apply to all aspects of logic.
/// </summary>
public static class GlobalActions {

    /// <summary>
    /// The action to call when an ability or actions fails but was still activated.
    /// </summary>
    [ActionName("Fizzle")]
    public static void Fizzle() {
        // TODO CLIENT: Fizzle animations.
    }

}
