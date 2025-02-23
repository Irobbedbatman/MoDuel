using MoDuel.Shared;

namespace MoDuel;

/// <summary>
/// The log types unique to MoDuel.
/// </summary>
public static class LogTypes {

    /// <summary>
    /// Used when a trigger is called.
    /// </summary>
    public static readonly Logger.LogType TriggerCalled = new("Trigger Called", 10);
    /// <summary>
    /// Used when a trigger fails.
    /// </summary>
    public static readonly Logger.LogType TriggerFailed = new("Trigger Failed", 11);
    /// <summary>
    /// Used when the turn changes.
    /// </summary>
    public static readonly Logger.LogType TurnStatusUpdate = new("Turn Status Update", 12);
    /// <summary>
    /// Used when a flow command is received.
    /// </summary>
    public static readonly Logger.LogType FLowCommands = new("Flow Commands", 13);
    /// <summary>
    /// Used when the flow thread changes state.
    /// </summary>
    public static readonly Logger.LogType FlowThreadState = new("Flow Thread State", 14);
    /// <summary>
    /// Used when an outbound request is sent.
    /// </summary>
    public static readonly Logger.LogType OutboundRequests = new("Outbound Requests", 15);
    /// <summary>
    /// Used when a error when the state becomes invalid.
    /// </summary>
    public static readonly Logger.LogType StateError = new("State Error", 16);
    /// <summary>
    /// Used when an action function fails to execute.
    /// </summary>
    public static readonly Logger.LogType ActionFailed = new("Action Failed", 17);

}
