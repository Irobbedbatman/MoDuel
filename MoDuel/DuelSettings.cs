using MoDuel.Serialization;
using MoDuel.Time;

namespace MoDuel;

/// <summary>
/// Settings that define how a duel will operate.
/// <para>Including animation settings and optional <see cref="ActionFunction"/>s for lua based operations at key points.</para>
/// </summary>
[SerializeReference]
public class DuelSettings {

    /// <summary>
    /// The seed used by the <see cref="ManagedRandom"/>. Set to null for a random value.
    /// </summary>
    public int? RandomSeed;
    /// <summary>
    /// Makes the <see cref="Player"/> with the same ID to go first.
    /// <para>If the id could not be found a random one will be used instead.</para>
    /// </summary>
    public string ForceIdToGoFirst = "";
    /// <summary>
    /// A multiplier applied to any delay on playback.
    /// <para>Interruptions can still occur. Doesn't affect sections not blocked.</para>
    /// <para></para>
    /// </summary>
    public float BlockPlaybackDurationMultiplier = 1;
    /// <summary>
    /// Shorthand check to see if animations should play.
    /// </summary>
    public bool IsPlaybackBlocked => BlockPlaybackDurationMultiplier == 0;
    /// <summary>
    /// The settings used to handle player timeouts.
    /// <para>If no settings are provided not timeouts will occur.</para>
    /// </summary>
    public TimerSettings TimerSettings = TimerSettings.NoTimeout;

    /// <summary>
    /// Action that is called when the game starts.
    /// </summary>
    public ActionFunction GameStartAction = new();

    /// <summary>
    /// Action that is called when the game ends.
    /// </summary>
    public ActionFunction GameEndAction = new();

    /// <summary>
    /// A fallback action that will be called when a <see cref="Cards.CardInstance"/> doesn't have unique behaviour for the same named trigger.
    /// </summary>
    public ActionFunction
        CardGetDisplayedLevelFallback = new(),
        CardGetDisplayedAttackFallback = new(),
        CardGetDisplayedArmourFallback = new(),
        CardGetDisplayedLifeFallback = new(),
        CardGetDisplayedCostFallback = new(),
        CardGetDisplayedDescriptionFallback = new();

}
