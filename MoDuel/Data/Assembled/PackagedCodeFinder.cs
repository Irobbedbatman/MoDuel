using System.Reflection;

namespace MoDuel.Data.Assembled;

/// <summary>
/// Helper class to help find and initialize <see cref="PackagedCode"/> in external packages.
/// </summary>
public static class PackagedCodeFinder {

    /// <summary>
    /// The property in the package data that details the path to the assembly file.
    /// </summary>
    public const string ASSEMBLY_PROPERTY = "Assembly";

    /// <summary>
    /// Loads the <see cref="PackagedCode"/> from the provided <paramref name="sourcePackage"/>.
    /// </summary>
    /// <param name="sourcePackage"></param>
    /// <returns>The loaded <see cref="PackagedCode"/> or an empty packaged code if there was an error.</returns>
    public static PackagedCode FindAndInit(Package sourcePackage) {

        // Get the assembly through the package.
        var assemblyPath = sourcePackage.GetFilePropertyPath(ASSEMBLY_PROPERTY);
        if (assemblyPath == null)
            return PackagedCode.CreateEmptyPackagedCode(sourcePackage);
        var assembly = Assembly.LoadFrom(assemblyPath);

        var info = assembly.GetCustomAttribute<PackageAssemblyAttribute>();
        if (info == null)
            return PackagedCode.CreateEmptyPackagedCode(sourcePackage);

        Type? type = info.PackageType;

        // Build the type into the usable packaged code.
        if (typeof(PackagedCode).IsAssignableFrom(type)) {
            // TODO PERFORMANCE: Consider other ways to call the constructor that may be more performant.
            object? packCode = Activator.CreateInstance(type, sourcePackage);
            if (packCode is PackagedCode result) {
                PackageAssemblyAttribute.Register(result);
                return result;
            }
        }

        return PackagedCode.CreateEmptyPackagedCode(sourcePackage);
    }

}
