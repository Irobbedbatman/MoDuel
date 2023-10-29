using MoDuel.Players;

namespace MoDuel.Flow;

/// <summary>
/// The subcomponent of the <see cref="DuelFlow"/> that manages incoming commands.
/// </summary>
internal class FlowCommandHandler : IDisposable {

    /// <summary>
    /// The duration in seconds a <see cref="CommandReference"/> is eligble to be activated.
    /// <para>This doesn't remove it from the queue; rather makes the command have no effect.</para>
    /// </summary>
    public const double CommandTimeOut = 1;

    /// <summary>
    /// Wether managed resources have been disposed.
    /// </summary>
    private bool isDisposed;

    /// <summary>
    /// The reset event that is called when a command has been recieved so as not to block the thread while waiting.
    /// </summary>
    private ManualResetEvent? CommandReadySignal = new(false);

    /// <summary>
    /// The list of commands buffered by players.
    /// <para>Sorted by the time they were added.</para>
    /// <para>Only one command should be stored per <see cref="Player"/>; theeir older command will be replaced.</para>
    /// </summary>
    private readonly SortedList<CommandReference, Action> CommandBuffer = new();

    /// <summary>
    /// Retrieves a command from a <paramref name="player"/> and adds it to the <see cref="CommandBuffer"/>.
    /// <para>Each player can only have one command buffered at a time.</para>
    /// <para>Signals the <see cref="CommandReadySignal"/>.</para>
    /// </summary>
    /// <param name="cmdId">The key used to find the command to run.</param>
    /// <param name="player">The <see cref="Player"/> that send the command.</param>
    /// <param name="args">The args that will passed to the command.</param>
    public void EnqueueCommand(Player player, ActionFunction action, params object?[] args) {

        // The time this command was added.
        DateTime commandTime = DateTime.UtcNow;

        // The wrapped function we use as a command.
        void command() {

            var span = DateTime.UtcNow - commandTime;
            // If the command was created more than a second before execution it is ignored. System commands don't have this limitation.
            if (span.TotalSeconds > CommandTimeOut) {
                if (DuelFlow.LoggingEnabled)
                    Console.WriteLine($"Command timed out. It was buffered for {span.TotalSeconds} seconds");
                return;
            }

            // Execute the command and get a return code that represents if the command was valid.
            bool commandSucess = action.Call(args.Prepend(player).ToArray());
            // TODO: VALIDATION utilise command failure.
        }

        // Lock commands so that remove of commands is still thread safe.
        lock (CommandBuffer) {

            // Add the command to the buffer.
            var cmdRef = new CommandReference(player, commandTime);
            // As command refernces are equal for the same player this will overwrite any old commands.
            CommandBuffer[cmdRef] = command;

            if (DuelFlow.LoggingEnabled)
                Console.WriteLine("Command Recieved");

            // Tell the flow to resume.
            CommandReadySignal?.Set();

        }
    }


    /// <summary>
    /// Tries to run the command that is first in the <see cref="CommandBuffer"/>.
    /// <para>Ensures execution is thread safe.</para>
    /// </summary>
    public void DequeueCommandAndRun() {

        // Record a reference to the command that will retrieved from the command buffer.
        KeyValuePair<CommandReference, Action>? command = null;

        // We lock for removal and retreival.
        lock (CommandBuffer) {
            if (CommandBuffer.Count > 0) {
                // Get the command.
                command = CommandBuffer.FirstOrDefault();
                if (command != null) {
                    // Remove the command from the command buffer.
                    CommandBuffer.Remove(command.Value.Key);
                }
            }
        }

        try {
            // Invoke the command and allow commands to be added while its is running.
            if (DuelFlow.LoggingEnabled && command != null)
                Console.WriteLine("Running provided command.");
            command?.Value.Invoke();
        }
        catch (Exception e) {
            // Because invoke may cause unhandled execptions display them for debugging.
            Console.Write(e.StackTrace);
        }
    }


    /// <summary>
    /// Blocks the current thread until the <see cref="CommandBuffer"/> and the <see cref="CommandReadySignal"/> is set.
    /// <para>If the command buffer has a value no blocking occurs.</para>
    /// </summary>
    public void WaitUntilCommandReady() {

        // Lock the buffer in case the ready signal is signaled between now and reset.
        lock (CommandBuffer) {

            // If the command buffer has a value no reason to block.
            if (CommandBuffer.Count > 0)
                return;

            if (DuelFlow.LoggingEnabled)
                Console.WriteLine("No Commands are ready. Waiting for one.");

            // Reset the signal so that WaitOne will block.
            CommandReadySignal?.Reset();

        }

        // Wait until a command is signaled.
        CommandReadySignal?.WaitOne();

    }

    /// <summary>
    /// Force the <see cref="FlowCommandHandler"/> into the command ready state.
    /// <para>This will stop the thread that is using it from being blocked.</para>
    /// </summary>
    public void ForceReadyState() {
        if (DuelFlow.LoggingEnabled) {
            Console.WriteLine("Forcing the ready state on the command handler.");
        }
        CommandReadySignal?.Set();
    }

    /// <summary>
    /// Disposes of the <see cref="CommandReadySignal"/> and sets it's value to null so that a new one can take it's place.
    /// </summary>
    private void DisposeSignal() {
        // Ensure there is a signal before disposing of it.
        if (CommandReadySignal != null) {
            CommandReadySignal.Dispose();
            CommandReadySignal = null;
        }
        else if (DuelFlow.LoggingEnabled) {
            Console.WriteLine("Attempt to dispose of command queue signal when it was already disposed.");
        }
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing) {
        if (!isDisposed) {
            // Free managed resources.
            if (disposing) {
                CommandBuffer.Clear();
            }
            // Free unmanaged resources.
            DisposeSignal();
            isDisposed = true;
        }
    }

    /// <summary>
    /// Finalizer that is used dispose of the ready signal.
    /// </summary>
    ~FlowCommandHandler() {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
