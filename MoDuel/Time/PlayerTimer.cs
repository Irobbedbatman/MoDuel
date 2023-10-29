using MoDuel.Players;
using MoDuel.Serialization;
using System.Diagnostics;
using System.Timers;

namespace MoDuel.Time;

/// <summary>
/// A wrapper for <see cref="System.Timers.Timer"/> that will reflect the amount of time a <see cref="Player"/> has spent on their turns.
/// <para>Settings are provided through the <see cref="TimerSettings"/>.</para>
/// </summary>
[SerializeReference]
public class PlayerTimer {

    /// <summary>
    /// The <see cref="Player"/> that this <see cref="PlayerTimer"/> belongs to.
    /// </summary>
    public readonly Player Player;
    /// <summary>
    /// The <see cref="TimerSettings"/> that the <see cref="PlayerTimer"/> will adjust time based off.
    /// </summary>
    private readonly TimerSettings Settings;
    /// <summary>
    /// The <see cref="Stopwatch"/> that will record the time between <see cref="Start(bool)"/> and <see cref="Stop()"/>.
    /// </summary>
    [MessagePack.IgnoreMember]
    private readonly Stopwatch TimeTakenWatch = new();
    /// <summary>
    /// The abosloute amount of time spent by the owning <see cref="Player"/>.
    /// <para>Compared against <see cref="TimerSettings.AbsolouteTimeOut"/>.</para>
    /// </summary>
    public double TotalElapsed { get; private set; } = 0;
    /// <summary>
    /// The allocated time this player has remainimg.
    /// <para>Uses <see cref="TimerSettings"/> to refresh the value each time.</para>
    /// </summary>
    public double TimeSaved { get; private set; } = 0;
    /// <summary>
    /// If <see cref="StartTimer(Timer)"/> has been called but <see cref="EndTimer(Timer)"/> has not.
    /// </summary>
    public bool Started { get; private set; } = false;
    /// <summary>
    /// A multiplier applied to <see cref="TimerSettings.Refresh"/>.
    /// <para>The value is multiplied with the value that <see cref="TimerSettings.PercentileDeficit"/> is calculated to be after each turn </para>
    /// <para>A player that uses 50% of their turn twice will result in a <see cref="DeficitMultiplier"/> of: 1 x .95 x .95. If <see cref="TimerSettings.PercentileDeficit"/> is 10%.</para>
    /// </summary>
    public double DeficitMultiplier { get; private set; } = 1;
    /// <summary>
    /// The <see cref="System.Timers.Timer"/> that this <see cref="PlayerTimer"/> uses to handle events.
    /// <para>This timer is disposed of when the <see cref="PlayerTimer"/> is finalized and as such isn't provided access.</para>
    /// </summary>
    private readonly System.Timers.Timer Timer = new() { AutoReset = false };

    public PlayerTimer(Player player, TimerSettings settings) {
        Player = player;
        Settings = settings;
        TimeSaved = settings.InitialTime;
    }

    /// <summary>
    /// Finalizer that disposes of unamanged resources.
    /// </summary>
    ~PlayerTimer() {
        try {
            Timer.Dispose();
        }
        catch { }
    }

    /// <summary>
    /// Assign the action that will occur when the <see cref="PlayerTimer"/> reaches 0.
    /// </summary>
    /// <param name="threadlock">The lockable object that is used to esnure the timer is thread safe.</param>
    public void SetTimeout(object threadlock) {
        Timer.Elapsed += delegate {
            lock (threadlock) {
                // Ensure the timer was not stoppped between the elapsed event and recieving the lock.
                if (Started) {
                    Settings.TimeOutAction?.Call(Player);
                }
            }
        };
    }

    /// <summary>
    /// Starts the <see cref="PlayerTimer"/>. Can be used as start or resume.
    /// </summary>
    /// <param name="refreshTimer">Should the timer <see cref="Refresh"/> before starting..</param>
    /// <returns>The amount of time until the timeout will occur.</returns>
    public double Start(bool refreshTimer = false) {

        // Ensure only one timer is active at a time.
        if (Started)
            return -1;

        // Check to see if the timer is set to not timeout.
        if (Settings == TimerSettings.NoTimeout)
            return -1;

        // If a refresh was requested; refresh the timer before starting.
        if (refreshTimer)
            Refresh();
        // Use the time remaining check if there is no refresh. Likely due to simply pausing the timer.
        else
            ModifyInterval(GetTimeRemaining() + Settings.Buffer);
        // Start the timer.
        Timer.Start();
        // Start the watch that will be recorded when the timer ends.
        TimeTakenWatch.Restart();
        //Mark the timer as started; ensuring that only one timer is ongoing and validate stopping the timer.
        Started = true;
        // Return how long the timer will go for minus without accoudning for the buffer.
        return Timer.Interval - Settings.Buffer;
    }

    /// <summary>
    /// Stops the <see cref="PlayerTimer"/> if has been <see cref="Started"/>.
    /// <para>Reduces <see cref="TimeSaved"/> by the time diffrence since <see cref="Start(bool)"/> was called.</para>
    /// <para>Also updates the <see cref="DeficitMultiplier"/>.</para>
    /// </summary>
    /// <returns>The time taken since <see cref="Start(bool)"/> was called.</returns>
    public double Stop() {

        // Ensure a timer was started.
        if (!Started)
            return -1;

        // Get the time since the timer started.
        double timeTaken = TimeTakenWatch.ElapsedMilliseconds;

        // Caclulate the percentage of time taken.
        double timeRate = timeTaken / TimeSaved;
        // Multiply the deficit multiplier inversly based on time spent. More time spent = less time refreshed.
        DeficitMultiplier *= 1 - (timeRate * Settings.PercentileDeficit);

        // Reduce the banked time by the time it took.
        TimeSaved -= timeTaken;
        // Increase total elapsed by the time taken.
        TotalElapsed += timeTaken;
        // Stop the timer.
        Timer.Stop();
        // Stop the watch that was recording the length of time the timer took.
        TimeTakenWatch.Reset();
        // Inform other aspects of the timer that it is stopped.
        Started = false;

        // Return the time elasped since Start was called.
        return timeTaken;
    }

    /// <summary>
    /// Increments the <see cref="TimeSaved"/> but only if the time is less than the <see cref="TimerSettings.MinRefresh"/>
    /// </summary>
    /// <returns>The new amount of time remaining on the timer.</returns>
    public double Refresh() {
        if (TimeSaved < Settings.MinRefresh)
            return Refresh(Settings.Refresh * DeficitMultiplier);
        return Timer.Interval - Settings.Buffer;
    }

    /// <summary>
    /// Increments the <see cref="TimeSaved"/> by the <paramref name="amount"/> provided.
    /// <para>Bypasses <see cref="TimerSettings.MinRefresh"/> but limits to <see cref="TimerSettings.MaxTime"/></para>
    /// </summary>
    /// <param name="amount">The amount to increment the timer by.</param>
    /// <returns>The new amount of time remaining on the timer.</returns>
    public double Refresh(double amount) {

        // Update the amount of time.
        TimeSaved += amount;

        // Limit the maximum amount of time.
        if (TimeSaved > Settings.MaxTime)
            TimeSaved = Settings.MaxTime;

        // Update the interval safely.
        ModifyInterval(GetTimeRemaining() + Settings.Buffer);

        // Return time remaining.
        return Timer.Interval - Settings.Buffer;
    }

    /// <summary>
    /// Updates the <see cref="Timer.Interval"/>.
    /// <para>If called while the timer is <see cref="Started"/> adjusts for the time spent.</para>
    /// <para>Adjusing the interval manually will reset the <see cref="Timer"/> and will desync <see cref="PlayerTimer"/> information.</para>
    /// </summary>
    /// <param name="newValue">The new <see cref="Timer.Interval"/> accounting for time spent since the timer was <see cref="Started"/>.</param>
    private void ModifyInterval(double newValue) {
        if (Started) {
            // If the timer is running need to reduce the interval by the time timer has been run for.
            double elapsed = TimeTakenWatch.ElapsedMilliseconds;
            double difference = newValue - elapsed;
            // If the timer would be run at an erroneous time run it for the minium amount of time.
            if (difference <= 0)
                difference = double.Epsilon;
            Timer.Interval = difference;
        }
        else {
            // Assign time noramlly when the timer is stopped.
            Timer.Interval = newValue;
        }
    }

    /// <summary>
    /// Resets all the values for the timer to their defaults.
    /// </summary>
    public void HardReset() {
        TotalElapsed = 0;
        TimeSaved = Settings.InitialTime;
        DeficitMultiplier = 1;
        Started = false;
        TimeTakenWatch.Reset();
        Timer.Stop();
    }

    /// <summary>
    /// Gets the time remaing until timeout.
    /// <para>Checks to see if <see cref="TimerSettings.AbsolouteTimeOut"/> will be reached before <see cref="TimeSaved"/> reahces 0.</para>
    /// </summary>
    public double GetTimeRemaining() {

        // TimeSaved does not update until after Stop is called.
        // So reduce it's value here by the time taken so far.
        double reduction = Started ? TimeTakenWatch.ElapsedMilliseconds : 0;
        // Check to see if absolouteTimeOut will happen before the saved time runs out.
        if (Settings.AbsolouteTimeOut - TotalElapsed < TimeSaved - reduction)
            return Settings.AbsolouteTimeOut - TotalElapsed;
        else
            return TimeSaved;

    }

}
