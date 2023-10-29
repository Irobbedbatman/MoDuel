using MoDuel.Data;
using MoDuel.Players;
using MoDuel.State;

namespace DefaultPackage;

/// <summary>
/// lThe category for actions that to apply to all aspects of logic.
/// </summary>
public static class GlobalActions {

    /// <summary>
    /// This is an example of using dynamics to not need to check the type of comparer.
    /// </summary>
    public static int CompareExample(dynamic comparer, object _, object __) {
        DuelState context = comparer.Context;
        Player prior = comparer.PriorityPlayer;
        string trigger = comparer.Trigger;
        return context.TurnCount + prior.Meta.UserId.Length + trigger.Length;
    }

    /// <summary>
    /// The action to call when an ability or actions fails but was still activated.
    /// </summary>
    [ActionName("Fizzle")]
    public static void Fizzle() {
        // TODO CLIENT: Fizzle aimations.
    }

}
