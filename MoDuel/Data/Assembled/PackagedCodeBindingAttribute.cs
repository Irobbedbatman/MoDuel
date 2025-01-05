namespace MoDuel.Data.Assembled;

/// <summary>
/// The <see cref="Attribute"/> that specifies the specific <see cref="PackagedCode"/> to be used for the package.
/// <para>This attribute is not required unless multiple types in a package derive from <see cref="PackagedCode"/>.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PackagedCodeBindingAttribute(string? packageName = null) : Attribute {

    /// <summary>
    /// If multiple packages are stored within the same assembly use this identifier to specify the entry for each package.
    /// </summary>
    public readonly string? PackageName = packageName;
}
