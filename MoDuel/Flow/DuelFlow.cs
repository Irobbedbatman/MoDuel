using MoDuel.Data;
using MoDuel.Players;
using MoDuel.State;
using MoDuel.Tools;

namespace MoDuel.Flow;

/// <summary>
/// The flow object that wraps a <see cref="DuelState"/> to provide interaction.
/// <para>After creation call <see cref="StartLoop"/> to start the duel.</para>
/// </summary>
public class DuelFlow : IDisposable {

    /// <summary>
    /// A flag that when enabled will cause the <see cref="DuelFlow"/> to print messages during operation.
    /// </summary>
    public static bool LoggingEnabled { get; set; } = false;

    /// <summary>
    /// The state the <see cref="DuelFlow"/> is providing flow to.
    /// </summary>
    public readonly DuelState State;

    /// <summary>
    /// The sole thread that the flow utilises for executing commands.
    /// <para>Null when not started.</para>
    /// </summary>
    private Thread? Thread = null;

    /// <summary>
    /// Has this object been disposed.
    /// </summary>
    private bool isDisposed;

    /// <summary>
    /// Wether the thread should be stopped.
    /// </summary>
    private bool Stopped = false;

    /// <summary>
    /// The packages that have been opened and tools used to access package content.
    /// </summary>
    public PackageCatalogue PackagedContent => State.PackageCatalogue;

    /// <summary>
    /// Action that is invoked when the thread is finished. Only allows for one value.
    /// </summary>
    public Action? OnThreadFinished { private get; set; }

    /// <summary>
    /// The <see cref="FlowCommandHandler"/> that will manage the command queue.
    /// </summary>
    private readonly FlowCommandHandler CommandHandler = new();

    public DuelFlow(DuelState state) {
        State = state;
    }

    /// <summary>
    /// Starts the duel loop if it had not already been started.
    /// </summary>
    /// <param name="startDuel">Wether to also start the duel. Note if the duel state is not started the loop will not start.</param>
    public void StartLoop(bool startDuel = true) {

        if (Thread != null) {
            Console.WriteLine("DuelFlow thread already started. Cannot start another.");
            return;
        }

        if (startDuel)
            State.Start();

        Thread = new(new ThreadStart(Loop));
        Stopped = false;
        Thread.Start();
    }

    /// <summary>
    /// Stops the duel loop.
    /// </summary>
    public void StopLoop() {
        Stopped = true;
        CommandHandler.ForceReadyState();
        Thread?.Join();
    }

    /// <summary>
    /// The game logic loop.
    /// </summary>
    private void Loop() {

        ThreadContext.DuelState = State;

        // Only lopp while the duel is ongoing.
        while (!Stopped && State.Ongoing) {

            // Non-blocking wait until a command is ready.
            CommandHandler.WaitUntilCommandReady();

            // If after the command is done the duel is over stop the loop.
            if (Stopped || State.Finished)
                break;

            // Try and run a command.
            CommandHandler.DequeueCommandAndRun();
        }

        State.CleanupOnGameFinished();

        if (LoggingEnabled)
            Console.WriteLine("Thread finished running cleanup.");

        // Perfrom clean up once the loop is over.
        OnThreadFinished?.Invoke();
        Thread = null;
    }


    /// <summary>
    /// Enqueues the commmand sent from the <paramref name="player"/> with the corresponding <paramref name="args"/>.
    /// </summary>
    /// <param name="cmdId">The key used to find the command to run.</param>
    /// <param name="player">The <see cref="Player"/> that send the command.</param>
    /// <param name="args">The args that will passed to the command.</param>
    public void EnqueueCommand(Player player, string cmdId, params object?[] args) {
        var action = PackagedContent.LoadAction(cmdId);
        CommandHandler.EnqueueCommand(player, action, args);
    }


    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing) {
        if (!isDisposed) {
            if (disposing) { }
            CommandHandler.Dispose();
            Thread?.Join();
            OnThreadFinished = null;
            isDisposed = true;
        }
    }

    /// <summary>
    /// Finalizer to clean up unmaaged resources.
    /// </summary>
    ~DuelFlow() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
