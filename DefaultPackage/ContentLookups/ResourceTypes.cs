using MoDuel.Resources;
using MoDuel.Tools;

namespace DefaultPackage.ContentLookups;

#nullable disable

/// <summary>
/// Lookup for resource types.
/// <para>These are thread resource and only work while the duel is ongoing.</para>
/// </summary>
public static class ResourceTypes {

    public static ResourceType ActionPoint => DefaultPackage.GetPackage().LoadResourceType(nameof(ActionPoint));

    public static ResourceType Void => DefaultPackage.GetPackage().LoadResourceType(nameof(Void));

}

#nullable enable

