namespace MoDuel.Serialization;

/// <summary>
/// Attribute that denotes that the type should be serialized as a refernce rather than as a value.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SerializeReferenceAttribute : Attribute { }
