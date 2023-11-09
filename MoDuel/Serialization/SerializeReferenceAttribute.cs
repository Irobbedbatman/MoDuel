namespace MoDuel.Serialization;

/// <summary>
/// Attribute that denotes that the type should be serialized as a reference rather than as a value.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SerializeReferenceAttribute : Attribute { }
