using System.Text.Json.Nodes;

namespace MoDuel.Json;

/// <summary>
/// Solely use to house the <see cref="DeadToken.Instance"/> which is a <see cref="JToken"/> that is missing.
/// </summary>
public static class DeadToken {

#nullable disable

    /// <summary>
    /// Token to use when json data is missing.
    /// </summary>
    public static readonly JsonValue Instance = JsonValue.Create("DEAD_TOKEN");

#nullable enable

}
