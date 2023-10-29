using MoDuel.Resources;
using MoDuel.Tools;

namespace DefaultPackage.ContentLookups;

#nullable disable

/// <summary>
/// Lookup for resource types.
/// <para>These are thread resource and only work while the duel is ongoing.</para>
/// </summary>
public static class ResourceTypes {

    public static ResourceType ActionPoints => ThreadContext.GetPackageInstance(DefaultPackage.Name).LoadResourceType(nameof(ActionPoints));

    public static ResourceType Void => ThreadContext.GetPackageInstance(DefaultPackage.Name).LoadResourceType(nameof(Void));

}

#nullable enable

