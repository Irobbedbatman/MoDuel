using MoDuel.Data;
using System.Reflection;

namespace MoDuel.Serialization;

/// <summary>
/// Interface for data asset that allows it to be reloaded instead of reserialized.
/// </summary>
public interface IReloadable {

    /// <summary>
    /// Gets the itemPath that will be used when reloading the file from a <see cref="PackageCatalogue"/>.
    /// <para>Returns null if the item should not be reloaded in this way.</para>
    /// <para><see cref="GetItemPath"/> should have precedence when it and <see cref="GetStaticMethod"/> are checked.</para>
    /// </summary>
    public string? GetItemPath() { return null; }

    /// <summary>
    /// Gets the method that will be used to get a object when called with that object expecting to be the same as this IReloadable.
    /// <para>Returns null if the the item should not be reloaded in this way.</para>
    /// <para><see cref="GetItemPath"/> should have precedence when it and <see cref="GetStaticMethod"/> are checked.</para>
    /// </summary>
    public MethodInfo? GetStaticMethod() {
        return null;
        // Example usage.
        //return typeof(IReloadable).GetMethod(nameof(ExampleStaticObjectReturn));
    }

}
