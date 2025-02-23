namespace MoDuel.Shared;

/// <summary>
/// General purpose logger.
/// </summary>
public static class Logger {

    /// <summary>
    /// The set of log types that will be output.
    /// </summary>
    public static readonly HashSet<int> LogIdsToOutput = [];

    /// <summary>
    /// Used for messages that do not fit another log type.
    /// </summary>
    public static readonly LogType General = new("General Message", 0);
    /// <summary>
    /// Used for messages when loading from a package.
    /// </summary>
    public static readonly LogType DataLoading = new("Data Loading", 1);
    /// <summary>
    /// Used for messages when there is an error loading from a package.
    /// </summary>
    public static readonly LogType DataLoadingError = new("Data Loading Error", 2);

    /// <summary>
    /// A type of log to output.
    /// </summary>
    public sealed class LogType {

        public readonly string Name;
        public readonly int Id;

        /// <summary>
        /// Create a new log type instance.
        /// </summary>
        /// <param name="name">The name of the log type.</param>
        /// <param name="id">The used to determine if this log should be output.</param>
        /// <param name="defaultToOutput">Should the log be added to the log handler by default. Default: true.</param>
        public LogType(string name, int id, bool defaultToOutput = true) {
            Name = name;
            Id = id;
            if (defaultToOutput)
                LogIdsToOutput.Add(id);
        }

    }

    /// <summary>
    /// The method used to log events.
    /// </summary>
    public static Action<string, LogType>? LogAction { get; set; } = (message, type) => Console.WriteLine(message);

    /// <summary>
    /// Logs a message of the type to the output.
    /// </summary>
    public static void Log(LogType type, string message) {
        if (LogIdsToOutput.Contains(type.Id)) {
            LogAction?.Invoke(message, type);
        }
    }

}
