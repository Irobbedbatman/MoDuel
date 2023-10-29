namespace MoDuel.Serialization;

/// <summary>
/// Extensions for use when using reflection, serializing or deserializing.
/// </summary>
public static class Extensions {

    /// <summary>
    /// Returns a name like <see cref="Type.AssemblyQualifiedName"/> without the version information.
    /// </summary>
    public static string GetSimpleQualifiedName(this Type type) {
        return type.FullName + ", " + type.Assembly.GetName().Name;
    }


}
