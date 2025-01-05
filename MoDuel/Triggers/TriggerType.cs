namespace MoDuel.Triggers;

/// <summary>
/// The various types of trigger that can be created.
/// </summary>
public enum TriggerType {

    /// <summary>
    /// A trigger that is directly set against a single target.
    /// </summary>
    Explicit,
    /// <summary>
    /// A trigger that is directly used to get data from a single target.
    /// </summary>
    ExplicitData,
    /// <summary>
    /// A trigger that checks against all listener.
    /// </summary>
    Implicit,
    /// <summary>
    /// A trigger that is used to override the retrieval of a data table.
    /// </summary>
    DataOverride,
    /// <summary>
    /// A trigger that is used to override the retrieval of a explicit data table.
    /// </summary>
    ExplicitDataOverride,
    /// <summary>
    /// A trigger that is used to validate a change to data.
    /// </summary>
    Validation,
    /// <summary>
    /// Should not trigger or be used in response to a trigger.
    /// </summary>
    None

}
