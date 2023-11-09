using MoDuel.Players;
using MoDuel.Serialization;

namespace MoDuel;

/// <summary>
/// The values of a given turn, created for each new turn.
/// </summary>
/// <param name="turnOwner">The player that will be taking the turn.</param>
/// <param name="lastTurn">The turn before the newly created one. Null if no turn preceded.</param>
[SerializeReference]
public class TurnData(Player turnOwner, TurnData? lastTurn = null) {

    /// <summary>
    /// The turn data that represents the turn before this one.
    /// </summary>
    public readonly TurnData? LastTurn = lastTurn;
    /// <summary>
    /// The player who is in control of the current turn.
    /// </summary>
    public readonly Player Owner = turnOwner;
    /// <summary>
    /// The UTC date and time when this turn started.
    /// </summary>
    public readonly DateTime TimeTurnStarted = DateTime.UtcNow;
    /// <summary>
    /// The span of time since <see cref="TimeTurnStarted"/> and now.
    /// </summary>
    public TimeSpan TimeElapsed => DateTime.UtcNow - TimeTurnStarted;
    /// <summary>
    /// Data that can be recorded for a turn.
    /// </summary>
    public readonly Dictionary<string, object> Data = [];
}
