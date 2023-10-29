using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.OngoingEffects;
using MoDuel.Players;
using MoDuel.State;

namespace MoDuel.Triggers;

/// <summary>
/// Comparer that provides the priority for multiple <see cref="IImplicitTriggerable"/>.
/// </summary>
public class TriggererPriorityComparer : IComparer<IImplicitTriggerable> {

    /// <summary>
    /// The player whose effect have more priority than other players.
    /// </summary>
    public readonly Player PriorityPlayer;

    /// <summary>
    /// The <see cref="DuelState"/> that is requesting the comparison.
    /// </summary>
    public DuelState Context => PriorityPlayer.Context;

    /// <summary>
    /// A lua based comparer that is used first in <see cref="Compare(Target, Target)"/>.
    /// </summary>
    public readonly ActionFunction? CustomComparer;

    /// <summary>
    /// The trigger that the <see cref="IImplicitTriggerable"/>s are reacting to.
    /// </summary>
    public readonly string Trigger;

    /// <summary>
    /// Comparer that provides the priority for multiple <see cref="IImplicitTriggerable"/>.
    /// </summary>
    /// <param name="priorityPlayer">A player that is marked as having more priority as other players.</param>
    /// <param name="customComparer">A commparer that is performed after implicit conversion.</param>
    public TriggererPriorityComparer(Player priorityPlayer, string trigger = "", ActionFunction? customComparer = null) {
        PriorityPlayer = priorityPlayer;
        CustomComparer = customComparer;
        Trigger = trigger;
    }

    /// <summary>
    /// Compares two triggerers with the following priorty.
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


        // Perform each subcomparison and if they are decisive use their result.
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
        if (!(x is CardInstance cardx && y is CardInstance cardy))
            return 0;

        // Use the specific comparison specifically for cards. Skipping explicit comparisons as it was perfromed here first.
        return new CardInstanceComparer(PriorityPlayer, Trigger).Compare(cardx, cardy, true);

    }

    /// <summary>
    /// Compares two <see cref="IImplicitTriggerable"/>s. If one of them is an <see cref="OngoingEffect"/> it has priority.
    /// <para>Global effects have priority of nonglobals.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    private static int EffectCompare(IImplicitTriggerable? x, IImplicitTriggerable? y) {
        if (x is OngoingEffect effectx) {
            if (y is OngoingEffect effecty) {
                if (effectx.IsGlobal && !effecty.IsGlobal)
                    return -1;
                if (effecty.IsGlobal)
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
    /// Compares two <see cref="IImplicitTriggerable"/>s priortising <see cref="HeroInstance"/>s.
    /// <para>If both <see cref="IImplicitTriggerable"/>s are <see cref="HeroInstance"/>s priortise their player owner through <see cref="PriorityPlayerCompare(Player, Player)"/>.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    private int HeroCompare(IImplicitTriggerable x, IImplicitTriggerable y) {
        if (x is HeroInstance herox) {
            if (y is HeroInstance heroy) {
                return PriorityPlayerCompare(herox.Owner, heroy.Owner);
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
    /// Compares two players and priortise the <see cref="Player"/> that is the <see cref="PriorityPlayer"/>.
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
    /// Comparese two <see cref="IImplicitTriggerable"/> though <see cref="IExplicitTriggerable"/>.
    /// <para>Performs the <see cref="IExplicitTriggerable.GetExplicitReaction(string)"/> with the key Compare.</para>
    /// </summary>
    /// <returns>-1 if x is higher priority.. +1 if y is higher priority. 0 for triggerers that are equivalent.</returns>
    public int ExplicitCompare(IImplicitTriggerable? x, IImplicitTriggerable? y) {
        // The reaction closures for each provided objects.
        ActionFunction? xClosure = null;
        ActionFunction? yClousre = null;

        // Try to get the reactions.
        if (x is IExplicitTriggerable xtriggerable)
            xClosure = xtriggerable.GetExplicitReaction("Compare");
        if (y is IExplicitTriggerable ytriggerable)
            yClousre = ytriggerable.GetExplicitReaction("Compare");

        // Value of both explicit reactions checked against each other.
        // THere is no guarantee that Compare(x,y) is inversly equal to Compare(y,x) nor does there need to be.
        int totalResult = 0;

        if (xClosure != null) {
            int? result = xClosure.Call(this, x, y);
            totalResult += result ?? 0;
        }

        if (yClousre != null) {
            int? result = yClousre.Call(this, y, x);
            totalResult -= result ?? 0;
        }

        return totalResult;
    }

}
