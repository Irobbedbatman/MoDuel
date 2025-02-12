using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Data.Assembled;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public  class PackageAssemblyAttribute : Attribute {

    /// <summary>
    /// The lookup for packages that have already been loaded.
    /// </summary>
    private static readonly Dictionary<Assembly, PackagedCode> RegisteredPackages = [];

    /// <summary>
    /// The type that will be initialized.
    /// </summary>
    public readonly Type PackageType;

    public PackageAssemblyAttribute(Type t) {
        PackageType = t;
    }

    /// <summary>
    /// Registered the provided <paramref name="packagedCode"/>. So that it can be retrieved in <see cref="GetPackage(Assembly)"/>.
    /// </summary>
    public static void Register(PackagedCode packagedCode) {
        RegisteredPackages.Add(packagedCode.GetType().Assembly, packagedCode);
    }

    /// <summary>
    /// Retrieves the <see cref="PackagedCode"/> registered to the provided <paramref name="assembly"/>.
    /// </summary>
    public static PackagedCode? GetPackage(Assembly assembly) {
        return RegisteredPackages.GetValueOrDefault(assembly);
    }

}
