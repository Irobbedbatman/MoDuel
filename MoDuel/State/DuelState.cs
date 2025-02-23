using MoDuel.Data;
using MoDuel.Players;
using MoDuel.Serialization;
using MoDuel.Shared;
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
/// <item>StateCommunication.cs | Communication outbound.</item>
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
    /// The <see cref="DuelSettingsLoaded"/> that will remain constant throughout the duel.
    /// </summary>
    public readonly DuelSettingsLoaded Settings;

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
    /// The player that will or did have the first turn.
    /// </summary>
    public readonly Player FirstPlayer;

    public DuelState(PlayerMetaLoaded player1, PlayerMetaLoaded player2, PackageCatalogue packageCatalogue, DuelSettingsLoaded settings) {
        _packageCatalogue = packageCatalogue;
        Settings = settings;
        Random = settings.RandomSeed.HasValue ? new(settings.RandomSeed.Value) : new();

        GlobalEntity = new GlobalEntity(this);

        Player1 = new(this, player1);
        Player2 = new(this, player2);

        // Get the first player by their id.
        FirstPlayer = GetPlayerByUserID(Settings.ForceIdToGoFirst) ?? Random.NextItemParams(Player1, Player2) ?? Player1;
        CurrentTurn = new(FirstPlayer);
        Field = new(this, Player1.Field, Player2.Field);

        // Add all the abilities to the global entity.
        foreach (var ability in Settings.GlobalAbilities) {
            GlobalEntity.AddAbility(new Abilities.AbilityReference(GlobalEntity, new Sources.SourceImprint(Settings), ability));
        }

        // Use the on duel loaded hook on all the packaged code.
        foreach (var package in _packageCatalogue) {
            package.PackagedCode.OnDuelLoaded(this);
        }

    }

    /// <summary>
    /// Puts the <see cref="DuelState"/> in a started status and calls the <see cref="DuelSettingsLoaded.GameStartAction"/>.
    /// </summary>
    internal void Start() {
        if (Started) {
            Logger.Log(LogTypes.StateError, "Failed to Start duel as it has already started.");
            return;
        }
        Started = true;
        Settings.GameStartAction.Call(this);
    }

    /// <summary>
    /// Calls the game clean up action if the duel is finished.
    /// </summary>
    internal void CleanUpOnGameFinished() {
        if (Finished)
            Settings.GameEndAction.Call(this);
    }

}
