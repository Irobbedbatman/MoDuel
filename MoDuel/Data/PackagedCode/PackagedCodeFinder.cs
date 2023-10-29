using System.Reflection;

namespace MoDuel.Data;

/// <summary>
/// Helper class to help find and initialize <see cref="PackagedCode"/> in external packages.
/// </summary>
public static class PackagedCodeFinder {

    /// <summary>
    /// The property in the package data that details the path to the assembly file.
    /// </summary>
    public const string ASSEMBLY_PROPERTY = "Assembly";

    /// <summary>
    /// The property in the package data that details the namespace path that the package code file is in,
    /// </summary>
    public const string NAMESPACE_PROPERTY = "Namespace";

    /// <summary>
    /// The property in the package data that details a custom name provided to the package code file.
    /// </summary>
    public const string PACKAGE_CLASS_PROPERTY = "ClassName";


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

        Type? type = null;
        // Check to see if the type should be aquired through it's parameters.
        if (sourcePackage.GetProperty<string>(PACKAGE_CLASS_PROPERTY) != null)
            type = GetEntryFromParamters(assembly, sourcePackage);
        // If type was loaded search through reflection.
        type ??= GetEntryThroughReflection(assembly, sourcePackage.Name);

        // Build the type into the usable packaged code.
        if (typeof(PackagedCode).IsAssignableFrom(type)) {
            // TODO PERFORMANCE: Consider other ways to call the constructor that may be more performant.
            object? pcode = Activator.CreateInstance(type, sourcePackage);
            if (pcode is PackagedCode result) {
                return result;
            }
        }

        return PackagedCode.CreateEmptyPackagedCode(sourcePackage);
    }

    /// <summary>
    /// Gets the <see cref="PackagedCode"/> type implemented by the package.
    /// <para>Perfroms checks to ensure the most accurate <see cref="PackagedCode"/> is returned.</para>
    /// <para>The behaviour of this method is directly affected by the <see cref="PackagedCodeBindingAttribute"/>.</para>
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="packageName">The package name property that can be passed the entry attribute to check for mactches.</param>
    /// <returns>The single type that best fits the expected <see cref="PackagedCode"/> implementation. Returns null if no type was found or if there were multiple types that couldn't be decided between. </returns>
    public static Type? GetEntryThroughReflection(Assembly assembly, string? packageName = null) {

        // Get the types that are packaged code.
        IEnumerable<Type> types = assembly.GetTypes();
        types = types.Where(type => type.IsAssignableTo(typeof(PackagedCode)));
        // No type could be found.
        if (!types.Any()) return null;
        // Check to see if only one type was found.
        if (!types.Skip(1).Any()) {
            return types.FirstOrDefault();
        }
        // Check for types with the entry attribute.
        types = types.Where(
            type => type.GetCustomAttribute<PackagedCodeBindingAttribute>() != null);
        // No type could be found.
        if (!types.Any()) return null;
        // Check to see if only one type was found.
        if (!types.Skip(1).Any()) {
            return types.FirstOrDefault();
        }
        // Check to see if the packagname is equivlant
        if (packageName == null)
            return null;
        types = types.Where((type) => {
            var attr = type.GetCustomAttribute<PackagedCodeBindingAttribute>();
            if (attr == null)
                return false;
            if (attr is PackagedCodeBindingAttribute entryAttr) {
                return entryAttr.PackageName == packageName;
            }
            return false;
        });
        // No type could be found.
        if (!types.Any()) return null;
        // Check to see if only one type was found.
        if (!types.Skip(1).Any()) {
            return types.FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Gets the <see cref="PackagedCode"/> type defined by the <paramref name="package"/> provided using attributes found within the <paramref name="package"/>.
    /// </summary>
    /// <returns>The found type that macthes the properties provided. Null if it couldn't be found or if the type was not a <see cref="PackagedCode"/>..</returns>
    /// <exception cref="TypeLoadException"></exception>
    public static Type? GetEntryFromParamters(Assembly assembly, Package package) {

        // Get the full type name.
        var typeNamespace = package.GetProperty<string>(NAMESPACE_PROPERTY);
        var typeClassName = package.GetProperty<string>(PACKAGE_CLASS_PROPERTY);

        // Add the namespace seperator if a namespace exists.
        if (typeNamespace != null) {
            typeNamespace += ".";
        }

        // Get the expected type location.
        var typeName = string.Concat(typeNamespace, typeClassName ?? package.Name);
        var type = assembly.GetType(typeName, false);
        if (type != null) { return type.IsAssignableTo(typeof(PackagedCode)) ? type : null; }
        // Use the package name as a namespace if we couldn't load it before.
        typeName = string.Concat(typeNamespace ?? package.Name, ".", typeClassName ?? package.Name);
        type = assembly.GetType(typeName, false);
        if (type != null) { return type.IsAssignableTo(typeof(PackagedCode)) ? type : null; }

        Console.WriteLine($"Failed to load the packged code type from {string.Concat(typeNamespace, typeClassName ?? package.Name)}");

        return null;
    }

}
