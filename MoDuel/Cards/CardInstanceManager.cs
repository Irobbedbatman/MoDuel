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
    private readonly List<CardInstance> _cardInstances = [];
    public IReadOnlyList<CardInstance> CardInstances => _cardInstances.AsReadOnly();

    public void Register(CardInstance ci) {
        _cardInstances.Add(ci);
    }

    public void Deregister(CardInstance ci) {
        _cardInstances.Remove(ci);
    }

}
