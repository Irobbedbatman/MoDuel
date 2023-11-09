namespace MoDuel.Tools;

/// <summary>
/// A handler for creating a block of time during which the thread will be freed and execution will be paused.
/// </summary>
public class PlaybackBlockingHandler : IDisposable {

    /// <summary>
    /// Check to see if there is currently an blocking happening.
    /// </summary>
    public bool IsBlocking { get; private set; } = false;

    /// <summary>
    /// How long the current blocking has to play for before finishing.
    /// </summary>
    public TimeSpan Elapsed => DateTime.UtcNow - TimeStarted;

    /// <summary>
    /// The utc time the current playback blocking started.
    /// </summary>
    public DateTime TimeStarted { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// The duration of the currently playback block.
    /// <para>Measured in milliseconds.</para>
    /// </summary>
    public double Duration { get; private set; } = 0;

    /// <summary>
    /// Cancelation token  source used to abruptly end the blocking.
    /// <para>Use <see cref="EndBlock"/> to use it.</para>
    /// </summary>
    private CancellationTokenSource? tokenSource;

    /// <summary>
    /// Starts blocking the thread so that an animation can happen before any further flow on the current thread.
    /// </summary>
    /// <param name="blockTime">How long to block the thread for in milliseconds.</param>
    /// <returns>Returns true if the full time was taken.</returns>
    public bool StartBlock(double blockTime) {
        //Ensure there isn't already an animation blocker in place..
        if (IsBlocking)
            return false;
        // Update the current blocking variables.
        TimeStarted = DateTime.UtcNow;
        Duration = blockTime;
        //Clear and recreate the token source so we can reuse it.
        tokenSource?.Dispose();
        tokenSource = new CancellationTokenSource();
        //Start a task that delays the current thread until the blocking has stopped.
        Task t = Task.Delay(TimeSpan.FromMilliseconds(blockTime), tokenSource.Token);
        try {
            IsBlocking = true;
            t.Wait();
            return true;
        }
        catch (AggregateException) {
            // Blocking Cancelled
            return false;
        }
        finally {
            // Either way the blocking finishes cleanup resources.
            IsBlocking = false;
            Dispose();
        }
    }

    /// <summary>
    /// Forces the animation blocking to end by stopping the task it uses.
    /// </summary>
    public void EndBlock() {
        tokenSource?.Cancel();
    }

    /// <inheritdoc/>
    public void Dispose() {
        // Force the blocker to end before disposal.
        if (IsBlocking) {
            IsBlocking = false;
            EndBlock();
        }
        tokenSource?.Dispose();
        tokenSource = null;
        GC.SuppressFinalize(this);
    }
}
