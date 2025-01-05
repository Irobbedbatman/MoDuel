namespace MoDuel;

/// <summary>
/// The global enabled log settings.
/// </summary>
public static class LogSettings {

    /// <summary>
    /// The different events that can be logged.
    /// </summary>
    [Flags]
    public enum LogEvents {
        LogAll = 1023,
        TriggerCalled = 1,
        TurnStatusUpdate = 2,
        FlowCommands = 4,
        FlowThreadState = 8,
        OutboundRequests = 16,
        DataLoading = 32,
        DataLoadingError = 64,
        State = 128,
        ActionFailed = 256,
        TriggerFailed = 512
    }

    /// <summary>
    /// The events that will be logged.
    /// </summary>
    public static LogEvents LoggedEvents { get; set; } = LogEvents.LogAll;

    /// <summary>
    /// The method used to log events.
    /// </summary>
    public static Action<string>? LogAction { get; set; } = Console.WriteLine;

    /// <summary>
    /// Log a message to expected output.
    /// </summary>
    public static void Log(string message) => LogAction?.Invoke(message);

    /// <summary>
    /// Logs a message if the type is <paramref name="eventType"/> is marked for logging.
    /// </summary>
    public static void LogEvent(string message, LogEvents eventType) {
        if (LoggedEvents.HasFlag(eventType)) {
            Log(message);
        }
    }

}
