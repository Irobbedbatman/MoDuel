using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.OngoingEffects;
using MoDuel.Players;
using MoDuel.State;

namespace MoDuel.Triggers;

/// <summary>
/// Comparer that provides the priority for multiple <see cref="IImplicitTriggerable"/>.
/// </summary>
/// <remarks>
/// Comparer that provides the priority for multiple <see cref="IImplicitTriggerable"/>.
/// </remarks>
/// <param name="priorityPlayer">A player that is marked as having more priority as other players.</param>
/// <param name="customComparer">A comparer that is performed after implicit conversion.</param>
public class TriggererPriorityComparer(Player priorityPlayer, string trigger = "", ActionFunction? customComparer = null) : IComparer<IImplicitTriggerable> {

    /// <summary>
    /// The player whose effect have more priority than other players.
    /// </summary>
    public readonly Player PriorityPlayer = priorityPlayer;

    /// <summary>
    /// The <see cref="DuelState"/> that is requesting the comparison.
    /// </summary>
    public DuelState Context => PriorityPlayer.Context;

    /// <summary>
    /// A lua based comparer that is used first in <see cref="Compare(Target, Target)"/>.
    /// </summary>
    public readonly ActionFunction? CustomComparer = customComparer;

    /// <summary>
    /// The trigger that the <see cref="IImplicitTriggerable"/>s are reacting to.
    /// </summary>
    public readonly string Trigger = trigger;

    /// <summary>
    /// Compares two triggerers with the following priority.
    /// <list type="number">
    /// <item>Explicit comparison through <see cref="ExplicitCompare(IImplicitTriggerable, IImplicitTriggerable)"/></item>
    /// <item>Custom comparison through <see cref="CustomComparer"/>.</item>
    /// <item>Priority Player's Hero</item>
    /// <item>Other Heroes</item>
    /// <item>Ongoing Effects through <see cref="EffectCompare(IImplicitTriggerable, IImplicitTriggerable)"/></item>
    /// <item>CardInstances through <see cref="CardInstanceComparer"/>.</item>
    /// </list>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    public int Compare(IImplicitTriggerable? x, IImplicitTriggerable? y) {


        // Perform each sub-comparison and if they are decisive use their result.
        // ---
        int explicitCompare = ExplicitCompare(x, y);
        if (explicitCompare != 0)
            return explicitCompare;
        int customCompare = CustomCompare(x, y);
        if (customCompare != 0)
            return customCompare;

        // Null values work in the custom comparers but ensure they are valid for the remaining components.
        if (x == null || y == null)
            return 0;

        int heroCompare = HeroCompare(x, y);
        if (heroCompare != 0)
            return heroCompare;
        int effectCompare = EffectCompare(x, y);
        if (effectCompare != 0)
            return effectCompare;
        // ---

        // No comparison behaviour defined for anything else but cards.
        if (!(x is CardInstance cardX && y is CardInstance cardY))
            return 0;

        // Use the specific comparison specifically for cards. Skipping explicit comparisons as it was performed here first.
        return new CardInstanceComparer(PriorityPlayer, Trigger).Compare(cardX, cardY, true);

    }

    /// <summary>
    /// Compares two <see cref="IImplicitTriggerable"/>s. If one of them is an <see cref="OngoingEffect"/> it has priority.
    /// <para>Global effects have priority of non-globals.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    private static int EffectCompare(IImplicitTriggerable? x, IImplicitTriggerable? y) {
        if (x is OngoingEffect effectX) {
            if (y is OngoingEffect effectY) {
                if (effectX.IsGlobal && !effectY.IsGlobal)
                    return -1;
                if (effectY.IsGlobal)
                    return 1;
                return 0;
            }
            // Only x is an effect.
            return -1;
        }
        // Only y is an effect.
        if (y is OngoingEffect)
            return 1;
        return 0;
    }

    /// <summary>
    /// Compares two <see cref="IImplicitTriggerable"/>s prioritising <see cref="HeroInstance"/>s.
    /// <para>If both <see cref="IImplicitTriggerable"/>s are <see cref="HeroInstance"/>s prioritise their player owner through <see cref="PriorityPlayerCompare(Player, Player)"/>.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    private int HeroCompare(IImplicitTriggerable x, IImplicitTriggerable y) {
        if (x is HeroInstance heroX) {
            if (y is HeroInstance heroY) {
                return PriorityPlayerCompare(heroX.Owner, heroY.Owner);
            }
            // Only x is a hero instance.
            return -1;
        }
        // Only y is a hero instance.
        if (y is HeroInstance)
            return 1;

        return 0;
    }

    /// <summary>
    /// Compares two players and prioritise the <see cref="Player"/> that is the <see cref="PriorityPlayer"/>.
    /// <para>If neither provided <see cref="Player"/> is the <see cref="PriorityPlayer"/> return 0.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    private int PriorityPlayerCompare(Player x, Player y) {
        if (x == PriorityPlayer)
            return -1;
        if (y == PriorityPlayer)
            return 1;
        return 0;
    }

    /// <summary>
    /// Calls the <see cref="CustomComparer"/> and returns it result after parsing it.
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerables that are equivalent.</returns>
    public int CustomCompare(IImplicitTriggerable? x, IImplicitTriggerable? y) {
        int? result = CustomComparer?.Call(this, x, y);
        return result ?? 0;
    }

    /// <summary>
    /// Compares two <see cref="IImplicitTriggerable"/> though <see cref="IExplicitTriggerable"/>.
    /// <para>Performs the <see cref="IExplicitTriggerable.GetExplicitReaction(string)"/> with the key Compare.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    public int ExplicitCompare(IImplicitTriggerable? x, IImplicitTriggerable? y) {
        // The reaction closures for each provided objects.
        ActionFunction? actionX = null;
        ActionFunction? actionY = null;

        // Try to get the reactions.
        if (x is IExplicitTriggerable triggerableX)
            actionX = triggerableX.GetExplicitReaction("Compare");
        if (y is IExplicitTriggerable triggerableY)
            actionY = triggerableY.GetExplicitReaction("Compare");

        // Value of both explicit reactions checked against each other.
        // THere is no guarantee that Compare(x,y) is inversely equal to Compare(y,x) nor does there need to be.
        int totalResult = 0;

        if (actionX != null) {
            int? result = actionX.Call(this, x, y);
            totalResult += result ?? 0;
        }

        if (actionY != null) {
            int? result = actionY.Call(this, y, x);
            totalResult -= result ?? 0;
        }

        return totalResult;
    }

}
