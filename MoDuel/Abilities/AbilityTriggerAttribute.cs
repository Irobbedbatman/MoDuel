using MoDuel.Triggers;

namespace MoDuel.Abilities;

/// <summary>
/// An attribute used on ability triggers to determine how it triggers.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Obsolete("No longer how this is used. However may consider using it again as it is quite practical.")]
public sealed class AbilityTriggerAttribute : Attribute {

    /// <summary>
    /// The trigger type of this ability trigger.
    /// </summary>
    public readonly TriggerType Type;

    public AbilityTriggerAttribute(TriggerType type = TriggerType.Implicit) {
        Type = type;
    }   

}
