namespace MoDuel.Shared;

/// <summary>
/// Settings that define how a duel will operate.
/// </summary>
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

    // TODO: Timer settings.

    /// <summary>
    /// THe key of the action that is called when the game starts.
    /// </summary>
    public string? GameStartActionItemPath = null;

    /// <summary>
    /// THe key of the action that is called when the game ends.
    /// </summary>
    public string? GameEndActionItemPath = null;

    /// <summary>
    /// The highest a trigger can be recursively called.
    /// </summary>
    public int MaxTriggerDepth = 22;

    /// <summary>
    /// The set of keys of abilities that will be provided to <see cref="MoDUel.State.GlobalEntity"/> on creation of the duel state.
    /// </summary>
    public readonly List<string> GlobalAbilityItemPaths = [];

    /// <summary>
    /// Clone the settings. THis will point to all the same actions.
    /// </summary>
    public void Clone() => MemberwiseClone();

}
