using MoDuel.Abilities;
using MoDuel.Cards;
using MoDuel.Heroes;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.Serialization;
using MoDuel.State;

namespace MoDuel.Triggers;
public class ReactionComparer : IComparer<TriggerReaction> {

    /// <summary>
    /// The set of ability holding entities types and the priority they have.
    /// <para>Made public in case a type needs to be added when a package is loaded.</para>
    /// </summary>
    public static readonly Dictionary<Type, int> EntityTypePriority = new() {
        { typeof(GlobalEntity), 100 },
        { typeof(ResourceType), 200 },
        { typeof(Player), 300 },
        { typeof(HeroInstance), 400 },
        { typeof(CardInstance), 500 }
    };

    /// <summary>
    /// The trigger that the abilities will react too.
    /// </summary>
    public Trigger Trigger;

    /// <summary>
    /// Get the state the trigger was called within.
    /// </summary>
    public DuelState State => Trigger.State;

    public ReactionComparer(Trigger trigger) {
        Trigger = trigger;
    }

    /// <inheritdoc/>
    public int Compare(TriggerReaction? x, TriggerReaction? y) {
        if (x == null || y == null) return 0;
        return CompareAbilities(x.Source.Ability, y.Source.Ability);
    }

    /// <summary>
    /// Compare the two ability references that are creating the reactions.
    /// </summary>
    private int CompareAbilities(AbilityReference x, AbilityReference y) {
        int result = ExplicitCompare(x, y);
        if (result != 0) return result;
        result = CompareEntity(x, y);
        if (result != 0) return result;
        return x.GetType().GetSimpleQualifiedName().CompareTo(y.GetType().GetSimpleQualifiedName());
    }

    /// <summary>
    /// Use the comparer innate to the ability.
    /// </summary>
    private int ExplicitCompare(AbilityReference x, AbilityReference y) => x.CompareTo(State, y);

    /// <summary>
    /// Compare the two entities that have the abilities.
    /// </summary>
    private int CompareEntity(AbilityReference abilityX, AbilityReference abilityY) {

        var x = abilityX.Holder;
        var y = abilityY.Holder;

        int result = CompareEntityType(x, y);
        if (result != 0) return result;

        switch (x) {
            case Player player: // Turn player has priority.
                if (State.CurrentTurn.Owner == player)
                    return -1;
                if (State.CurrentTurn.Owner == y)
                    return 1;
                return 0;
            case GlobalEntity:
                return 0; // Expected to be handled explicitly.
            case ResourceType rt:
                return rt.GetItemPath().CompareTo(((ResourceType)y).GetItemPath());
            case HeroInstance hero:
                return CompareHeroPlayer(State, hero, (HeroInstance)y);
            case CardInstance card:
                return new CardInstanceComparer(Trigger).Compare(card, (CardInstance)y);
            default:
                return 0; // Compare instead by the ability.
        }
    }

    /// <summary>
    /// Compare the types of the two entities using the <see cref="EntityTypePriority"/>.
    /// </summary>
    private static int CompareEntityType(IAbilityEntity a, IAbilityEntity b) {

        int aTypePriority = 100;
        if (EntityTypePriority.TryGetValue(a.GetType(), out int priority)) {
            aTypePriority = priority;
        }

        int bTypePriority = 100;
        if (EntityTypePriority.TryGetValue(b.GetType(), out priority)) {
            bTypePriority = priority;
        }

        return aTypePriority - bTypePriority;

    }

    /// <summary>
    /// Compare abilities on a hero by the <see cref="Player"/>> that is using the hero.
    /// </summary>
    private static int CompareHeroPlayer(DuelState state, HeroInstance a, HeroInstance b) {
        if (state.CurrentTurn.Owner == a.Owner) {
            return -1;
        }
        if (state.CurrentTurn.Owner == b.Owner) {
            return 1;
        }
        return 0;
    }

}
