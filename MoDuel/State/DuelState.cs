using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.Tools;

namespace MoDuel.State;


/// <summary>
/// The values used to define a duel at a certain or current state.
/// <para>All recordable duel information should be stored here so that it can be managed, stored or recreated.</para>
/// <para>This class is partial and spread across the files:</para>
/// <list type="bullet">
/// <item>DuelState.cs | The main contents and construction.</item>
/// <item>StateEntities.cs | Target data.</item>
/// <item>StateStatus.cs | The state machine.</item>
/// <item>StateTriggers.cs | The trigger reactions for the state.</item>
/// </list>
/// </summary>
[SerializeReference]
public partial class DuelState {

    /// <summary>
    /// The <see cref="Data.PackageCatalogue"/> files loaded for use in the duel can be found in.
    /// <para>This value is not serialized and needs to be set after deserialization.</para>
    /// <para>TODO: Consider having this in settings and have settings be settable in deserialization.</para>
    /// </summary>
    private PackageCatalogue _packageCatalogue;

    /// <summary>
    /// The <see cref="Data.PackageCatalogue"/> files loaded for use in the duel can be found in.
    /// </summary>
    public PackageCatalogue PackageCatalogue {
        get => _packageCatalogue;
        set => _packageCatalogue = value;
    }

    /// <summary>
    /// The <see cref="DuelSettings"/> that will remain constant throughout the duel.
    /// </summary>
    public readonly DuelSettings Settings;

    /// <summary>
    /// The <see cref="ManagedRandom"/> that any random actions should use.
    /// TODO FEATURE: consider moving this to settings so multiple games can share the same increment.
    /// </summary>
    public readonly ManagedRandom Random;

    /// <summary>
    /// Recordable game state values that are user defined.
    /// </summary>
    public readonly Dictionary<string, object?> Values = [];

    /// <summary>
    /// Accessor for <see cref="Values"/>.
    /// </summary>
    public object? this[string key] { get => Values[key]; set => Values[key] = value; }

    /// <summary>
    /// The animations that are sent out from the duel flow.
    /// </summary>
    public EventHandler<ClientRequest> OutBoundDelegate = delegate { };

    /// <summary>
    /// The player that will or did have the first turn.
    /// </summary>
    public Player FirstPlayer;

    public DuelState(PlayerMeta player1, PlayerMeta player2, PackageCatalogue packageCatalogue, DuelSettings settings) {
        _packageCatalogue = packageCatalogue;
        Settings = settings;
        Random = settings.RandomSeed.HasValue ? new(settings.RandomSeed.Value) : new();

        Player1 = new(this, player1);
        Player2 = new(this, player2);

        // Get the first player by their id.
        FirstPlayer = GetPlayerByUserID(Settings.ForceIdToGoFirst) ?? Random.NextItemParams(Player1, Player2) ?? Player1;
        CurrentTurn = new(FirstPlayer);
        Field = new(this, Player1.Field, Player2.Field);

        // Use the on duel loaded hook on all the packaged code.
        foreach (var package in _packageCatalogue) {
            package.PackagedCode.OnDuelLoaded(this);
        }

    }

    /// <summary>
    /// Puts the <see cref="DuelState"/> in a started status and calls the <see cref="DuelSettings.GameStartAction"/>.
    /// </summary>
    internal void Start() {
        if (Started) {
            Console.WriteLine("Failed to Start duel as it has already started.");
            return;
        }
        Started = true;
        Settings.GameStartAction.Call(this);
    }

    /// <summary>
    /// Calls the game clean up action if the duel is finished.
    /// </summary>
    internal void CleanupOnGameFinished() {
        if (Finished)
            Settings.GameEndAction.Call(this);
    }

}
