using MoDuel.Abilities;
using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Field;
using MoDuel.Flow;
using MoDuel.Heroes;
using MoDuel.Resources;
using MoDuel.Serialization;
using MoDuel.Shared.Structures;
using MoDuel.State;
using MoDuel.Time;
using MoDuel.Tools;

namespace MoDuel.Players;

/// <summary>
/// One of the players of the duel.
/// <para>Stores the players duel values.</para>
/// <para><see cref="UserId"/> should be made unique but is not confirmed to be as such. Use it's <see cref="Target.Index"/> to accurately determine the player.</para>
/// </summary>
[SerializeReference]
public class Player : Target, IAbilityEntity {

    /// <summary>
    /// Player that can be used to bypass command queue to perform actions.
    /// <para>Any running code will still need to finish first.</para>
    /// <para>Priority is defined through <see cref="CommandReference.CompareTo(CommandReference)"/>.</para>
    /// <para>This player is not state relative and uses constructed data that may break if used as a generic player.></para>
    /// </summary>
    internal static readonly Player SysPlayer = new("System");

    /// <summary>
    /// The string prefix to access <see cref="PrivateValues"/> when in lua.
    /// <para>Example Player.~Value</para>
    /// </summary>
    public const string PRIVATE_INDEX_PREFIX = "p_";

    /// <summary>
    /// The unique id, name, username of this player.
    /// </summary>
    public readonly string UserId;

    /// <summary>
    /// The <see cref="Resources.ResourcePool"/> that this player has.
    /// </summary>
    public readonly ResourcePool ResourcePool;

    /// <summary>
    /// A version of <see cref="Target.Values"/> that will not be serialized unless requested by this <see cref="Player"/>.
    /// </summary>
    public readonly Dictionary<string, object?> PrivateValues = [];

    /// <summary>
    /// The turn that the player had last this excludes their current turn.
    /// </summary>
    public TurnData? LastTurn;

    /// <summary>
    /// The abilities provided to the player that are separate from their hero.
    /// </summary>
    public readonly List<AbilityReference> Abilities = [];

    /// <summary>
    /// Indexer that allows for access to <see cref="Target.Values"/> and <see cref="PrivateValues"/>. 
    /// <para>Prefix with <see cref="PRIVATE_INDEX_PREFIX"/> to access <see cref="PrivateValues"/>.</para>
    /// </summary>
    /// <param name="key">The key from either collection.</param>
    /// <returns>The requested object or null if it couldn't be found.</returns>
    public new object? this[string key] {

        get {
            // Get from private values.
            if (key.StartsWith(PRIVATE_INDEX_PREFIX, StringComparison.CurrentCultureIgnoreCase)) {
                // Ensure the value exists. If it doesn't attempt to use the Target.Values.
                if (PrivateValues.TryGetValue(key, out var value))
                    return value;
                return base[key];
            }
            else
                return base[key];
        }
        set {
            // Set to private values.
            if (key.StartsWith(PRIVATE_INDEX_PREFIX, StringComparison.CurrentCultureIgnoreCase)) {
                PrivateValues[key] = value;
            }
            base[key] = value;
        }
    }

    /// <summary>
    /// The animations that are sent out for this specific player.
    /// </summary>
    public EventHandler<ClientRequest> OutBoundDelegate = delegate { };

    /// <summary>
    /// The event handler for when the player says they are ready after a <see cref="ClientRequest"/> marked as <see cref="ClientRequest.SendReadyConfirmation"/>.
    /// </summary>
    public EventHandler InBoundReadDelegate = delegate { };

    /// <summary>
    /// The <see cref="HeroInstance"/> this player is currently playing as.
    /// </summary
    public HeroInstance Hero;

    /// <summary>
    /// The <see cref="SubField"/> on this players side.
    /// </summary>
    public readonly SubField Field;

    /// <summary>
    /// One of the stats for this player.
    /// </summary>
    public int Level = 1, Exp = 0, Life = 25, MaxLife = 25;

    /// <summary>
    /// The Hand of the player currently.
    /// </summary>
    public readonly CardSet Hand;

    /// <summary>
    /// The Grave of the player currently.
    /// </summary>
    public readonly CardSet Grave;
    /// <summary>
    /// The state in which this player exists.
    /// </summary>
    public readonly DuelState Context;

    /// <summary>
    /// The meta that was used to create this player.
    /// </summary>
    public readonly PlayerMeta Meta;

    /// <summary>
    /// The timeout object for this player.
    /// </summary>
    public PlayerTimer? Timer;

    /// <summary>
    /// The amount of action points this player has.    
    /// </summary>
    public int ActionPoints = 0;

    /// <summary>
    /// Does the player stull have life points.
    /// </summary>
    public bool IsAlive => Life > 0;

#nullable disable
    private Player(string userId) : base(new TargetRegistry(new Indexer())) {
        if (userId != "System")
            throw new ArgumentException($"{nameof(userId)} must be System to ensure this constructor is not used.");
        UserId = userId;
    }
#nullable enable

    public Player(DuelState context, PlayerMeta meta) : base(context.TargetRegistry) {
        UserId = meta.UserId;
        Meta = meta;
        Context = context;
        ResourcePool = new ResourcePool(meta.ResourcePoolTypes);
        Field = new SubField(this);
        Hero = new HeroInstance(meta.Hero, this);
        Hand = new CardSet(this, "Hand");
        Grave = new CardSet(this, "Grave");
    }

    /// <summary>
    /// Assigns a <see cref="Timer"/> to this player if it did not already have one.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="threadLock"></param>
    public void AssignTimer(TimerSettings settings, object threadLock) {
        if (Timer != null)
            return;
        Timer = new PlayerTimer(this, settings);
        Timer.SetTimeout(threadLock);
    }

    /// <summary>
    /// Adds a card to this player's hand.
    /// </summary>
    /// <param name="card"></param>
    public void AddCardToHand(CardInstance card) => Hand.AddToLocation(card);

    /// <summary>
    /// Removes a card from this player's hand if it currently in there.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCardFromHand(CardInstance card) => Hand.RemoveFromLocation(card);

    /// <summary>
    /// Adds a card to this player's grave.
    /// </summary>
    /// <param name="card"></param>
    public void AddCardToGrave(CardInstance card) => Grave.AddToLocation(card);

    /// <summary>
    /// Removes a card from this player's grave if it currently in there.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCardFromGrave(CardInstance card) => Grave.RemoveFromLocation(card);

    /// <summary>
    /// Check to see if player has a certain card in their hand.
    /// </summary>
    public bool IsCardInHand(CardInstance card) => Hand.Contains(card);

    /// <summary>
    /// Check to see if player has a certain card in their grave.
    /// </summary>
    public bool IsCardInGrave(CardInstance card) => Grave.Contains(card);

    /// <summary>
    /// Sets the hero value by creating a new <see cref="HeroInstance"/> from the provided <see cref="Hero"/>.
    /// </summary>
    public void SetHero(Hero newHero) => Hero = new HeroInstance(newHero, this);

    /// <summary>
    /// Invokes <see cref="OutBoundDelegate"/> with a request for the client to do.
    /// <para>Call <see cref="BlockPlayback(double)"/> afterward if the request should stop other things from happening.</para>
    /// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    public void SendRequest(ClientRequest request) {
        OutBoundDelegate?.Invoke(this, request);
    }

    /// <summary>
    /// Sends a request through the <see cref="OutBoundDelegate"/> and blocks playback on the current thread until a ready check comes though the <see cref="InBoundReadDelegate"/>.
    /// <para>If this check is not performed before the <paramref name="timeout"/> occurs the playback will resume as well; returning true.. </para>
    /// </summary>
    /// <param name="request">THe request to be sent to the player.</param> 
    /// <param name="timeout">The amount time that playback will be blocked waiting for a ready response.</param>
    /// <returns>True if time reached the full <paramref name="timeout"/> otherwise false if the player sent the ready response.</returns>
    public bool SendBlockingRequest(ClientRequest request, double timeout) {
        // The blocker use to block the current thread.
        var blocker = new PlaybackBlockingHandler();
        // Listen for the ready response.
        InBoundReadDelegate = delegate {
            blocker.EndBlock();
            InBoundReadDelegate = delegate { };
        };
        // Send the request to the player.
        OutBoundDelegate?.Invoke(this, request);
        // Wait for the ready response.
        var result = blocker.StartBlock(timeout);
        InBoundReadDelegate = delegate { };
        return result;
    }

    /// <summary>
    /// Check to see if player had a certain card at the start of the game using <see cref="Card.Id"/>.
    /// </summary>
    public bool BroughtCard(string cardId) => Meta.Hand.Any((card) => card.Card.Id == cardId);

    /// <summary>
    /// Check to see if player had a certain card at the start of the game.
    /// </summary>
    public bool BroughtCard(Card card) => Meta.Hand.Any((handcard) => handcard.Card == card);

    /// <summary>
    /// Does this <see cref="Player"/> have control over the current turn.
    /// </summary>
    public bool IsTurnOwner() => Context.CurrentTurn.Owner == this;

    /// <summary>
    /// Get the opposing player via the state.
    /// </summary>
    public Player GetOpposingPlayer() {
        return Context.GetOpposingPlayer(this);
    }

    /// <inheritdoc/>
    public IEnumerable<AbilityReference> GetAbilities() => Abilities;


    /// <inheritdoc/>
    public DuelState GetState() => Context;

    /// <inheritdoc/>
    public void AddAbility(AbilityReference ability) => Abilities.Add(ability);

    /// <inheritdoc/>
    public void RemoveAbility(AbilityReference ability) => Abilities.Remove(ability);

    /// <summary>
    /// Calls a trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityTrigger(string triggerKey, DataTable data) => ((IAbilityEntity)this).Trigger(triggerKey, data);

    /// <summary>
    /// Calls a data trigger. This is currently implemented in the <see cref="IAbilityEntity"/>.
    /// </summary>
    public void AbilityDataTrigger<T>(string triggerKey, ref T data) where T : DataTable => ((IAbilityEntity)this).DataTrigger(triggerKey, ref data);

}
