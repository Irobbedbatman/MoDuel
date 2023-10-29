namespace MoDuel.Data;

/// <summary>
/// The attribute to attach to static methods to detail the name of the <see cref="ActionFunction"/>
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ActionNameAttribute : Attribute {

    /// <summary>
    /// The name of the action. This needs to be unique in each package as it searched for.
    /// </summary>
    public readonly string ActionName;

    public ActionNameAttribute(string actionName) {
        ActionName = actionName;
    }

}
