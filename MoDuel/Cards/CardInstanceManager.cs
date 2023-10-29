using MoDuel.Serialization;

namespace MoDuel.Cards;

/// <summary>
/// <see cref="CardInstanceManager"/> tells the <see cref="DuelFlow.FindReactions(string)"/> what cards are currently enabled for triggering.
/// </summary>
[SerializeReference]
public class CardInstanceManager {

    /// <summary>
    /// The list of all enabled <see cref="CardInstance"/> for triggering effects.
    /// </summary>
    private readonly List<CardInstance> _cardInstances = new();
    public IReadOnlyList<CardInstance> CardInstances => _cardInstances.AsReadOnly();

    public void Register(CardInstance ci) {
        _cardInstances.Add(ci);
    }

    public void Deregister(CardInstance ci) {
        _cardInstances.Remove(ci);
    }

    /// <summary>
    /// Returns the list of <see cref="CardInstances"/> sorted though use of the <paramref name="comparer"/>.
    /// </summary>
    /// <param name="comparer">The comparer that will order the output.</param>
    /// <param name="selector">The predicate used to filter out results. Return true to keep.</param>
    public IOrderedEnumerable<CardInstance> GetOrderedInstances(CardInstanceComparer comparer, Func<CardInstance, bool>? selector = null) {
        IEnumerable<CardInstance> instances = CardInstances;
        if (selector != null) {
            instances = instances.Where(selector);
        }
        return instances.OrderBy((item) => {
            return item;
        }, comparer);
    }

    /// <summary>
    /// Get the <see cref="CardInstances"/> that have a reaction to <paramref name="trigger"/>.
    /// <para>Result is sorted based on the <paramref name="comparer"/> provided.</para>
    /// </summary>
    /// <param name="comparer">The comparer used to order the result.</param>
    /// <param name="trigger">Onlt <see cref="CardInstance"/> that use this <paramref name="trigger"/> will be returned.</param>
    /// <param name="selector">The predicate used to filter out results. Return true to keep.</param>
    /// <returns>A sorted list of all the <see cref="CardInstance"/>s that have reactions to the provided <paramref name="trigger"/>.</returns>
    public SortedSet<CardInstance> GetOrderedReactors(CardInstanceComparer comparer, string trigger, Func<CardInstance, bool>? selector = null) {
        return new SortedSet<CardInstance>(
            CardInstances.Where((item) => {
                return item.HasImplicitReaction(trigger);
            }).Where(selector ?? ((ci) => true)),
            comparer
        );
    }

    /// <summary>
    /// Selector that returns true if the provided <see cref="CardInstance"/> is both explicitly and implictly active.
    /// </summary>
    public static bool ActiveSelector(CardInstance ci) => ci.Active;

    /// <summary>
    /// Selector that returns true if the provided <see cref="CardInstance"/> is implicit trigger active.
    /// </summary>
    public static bool ImplicitActiveSelector(CardInstance ci) => ci.ImplicitTriggerActive;

    /// <summary>
    /// Selector that returns true if the provided <see cref="CardInstance"/> is explicit trigger active.
    /// </summary>
    public static bool ExplicitActiveSelector(CardInstance ci) => ci.ExplicitTriggerActive;

    /// <summary>
    /// Selector that returns true if the provided <see cref="CardInstance"/> is a <see cref="CreatureInstance"/> and is alive.
    /// </summary>
    public static bool AliveCreatuesSelector(CardInstance ci) => ci is CreatureInstance creature && creature.IsAlive;


}
