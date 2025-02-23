using MoDuel.Abilities;
using MoDuel.Data;
using MoDuel.Shared;
using MoDuel.Time;
using System.Collections.Frozen;

namespace MoDuel;

/// <summary>
/// Settings that define how a duel will operate.
/// </summary>
public class DuelSettingsLoaded : DuelSettings {

    /// <summary>
    /// Create a new instance of the duel setting by cloning the values from the <paramref name="basis"/>.
    /// </summary>
    /// <param name="basis">The settings to copy from.</param>
    private DuelSettingsLoaded(DuelSettings basis) {
        var thisType = GetType();
        var otherType = basis.GetType();
        foreach (var field in otherType.GetFields()) {
            var value = field.GetValue(basis);
            thisType.GetField(field.Name)?.SetValue(this, value);
        }
        GameStartAction = new();
        GameEndAction = new();
        GlobalAbilities = new List<Ability>().ToFrozenSet();
    }

    /// <summary>
    /// Create a loaded version of the duel settings.
    /// </summary>
    /// <param name="basis">The basis that contains unloaded actions and abilities.</param>
    /// <param name="data">The packaged data to load actions from.</param>
    /// <returns>The newly created loaded settings or null if there was an issue during loading a resource.</returns>
    public static DuelSettingsLoaded? Load(DuelSettings basis, PackageCatalogue data) {

        ActionFunction gameStartAction;
        ActionFunction gameEndAction;

        if (basis.GameStartActionItemPath != null) {
            gameStartAction = data.LoadAction(basis.GameStartActionItemPath);
            if (!gameStartAction.Loaded) return null;
        }
        else {
            gameStartAction = new();
        }

        if (basis.GameEndActionItemPath != null) {
            gameEndAction = data.LoadAction(basis.GameEndActionItemPath);
            if (!gameStartAction.Loaded) return null;
        }
        else {
            gameEndAction = new();
        }

        List<Ability> abilities = [];

        foreach (var ability in basis.GlobalAbilityItemPaths) {
            var loadedAbility = data.LoadAbility(ability);
            if (loadedAbility == Ability.Missing ) return null;
            abilities.Add(data.LoadAbility(ability));
        }


        return new DuelSettingsLoaded(basis) {
            GameStartAction = gameStartAction,
            GameEndAction = gameEndAction,
            GlobalAbilities = abilities.ToFrozenSet()
        };

    } 



    /// <summary>
    /// The settings used to handle player timeouts.
    /// <para>If no settings are provided not timeouts will occur.</para>
    /// </summary>
    public TimerSettings TimerSettings = TimerSettings.NoTimeout;

    /// <summary>
    /// Action that is called when the game starts.
    /// </summary>
    public ActionFunction GameStartAction { private init; get; }

    /// <summary>
    /// Action that is called when the game ends.
    /// </summary>
    public ActionFunction GameEndAction { private init; get; }

    /// <summary>
    /// The set of abilities that will be provided to <see cref="State.GlobalEntity"/> on creation of the duel state.
    /// </summary>
    public FrozenSet<Ability> GlobalAbilities { private init; get; }

}
