using MoDuel.Players;
using MoDuel.Serialization;

namespace MoDuel.Time;


/// <summary>
/// Settings to be provided to <see cref="PlayerTimer"/> to define it's operation.
/// </summary>
[SerializeReference]
public class TimerSettings {

    /// <summary>
    /// The timer setting to use when no player timeouts will occur.
    /// </summary>
    public static readonly TimerSettings NoTimeout = new();

    /// <summary>
    /// The time this <see cref="Player"/> get's for all their turns collectively.
    /// <para>Doesn't account for time gained via refresh.</para>
    /// </summary>
    public double InitialTime = 0;
    /// <summary>
    /// The maximum amount of time remaining a <see cref="Player"/> can have.
    /// </summary>
    public double MaxTime = FromMinutes(2);
    /// <summary>
    /// How much time is restored for each of the <see cref="Player"/>s turns.
    /// </summary>
    public double Refresh = FromMinutes(1);
    /// <summary>
    /// How long the <see cref="Player"/>'s timer needs to be to trigger <see cref="Refresh"/>.
    /// </summary>
    public double MinRefresh = 0;
    /// <summary>
    /// A hard cutoff that ignore other time rules.
    /// </summary>
    public double AbsoluteTimeOut = FromMinutes(15);
    /// <summary>
    /// An amount of time that is added to the timer so that network or timer offset; such that it doesn't seem abrupt.
    /// </summary>
    public double Buffer = FromSeconds(0);
    /// <summary>
    /// An amount that should be removed from <see cref="Refresh"/> based of the amount of time taken.
    /// <para>If the value is 10% and the player used half their turn 5% of their next turn will be skipped.</para>
    /// </summary>
    public double PercentileDeficit = 0.1;
    /// <summary>
    /// The action invoked when the timer stops.
    /// </summary>
    public ActionFunction TimeOutAction = new();

    /// <summary>
    /// Converts a time from seconds to milliseconds.
    /// <para>All the values in <see cref="TimerSettings"/> use milliseconds.</para>
    /// </summary>
    public static double FromSeconds(double seconds) => seconds * 1000;

    /// <summary>
    /// Converts a time from minutes to milliseconds.
    /// <para>All the values in <see cref="TimerSettings"/> use milliseconds.</para>
    /// </summary>
    public static double FromMinutes(double minutes) => FromSeconds(minutes * 60);

}
