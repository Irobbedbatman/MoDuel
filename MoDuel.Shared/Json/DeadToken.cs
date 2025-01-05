using System.Text.Json.Nodes;

namespace MoDuel.Shared.Json;

/// <summary>
/// Solely used to house the <see cref="Instance"/> which is a <see cref="JToken"/> that is missing.
/// </summary>
public static class DeadToken {

    /// <summary>
    /// Token to use when json data is missing.
    /// </summary>
    public static readonly JsonValue Instance = JsonValue.Create("DEAD_TOKEN");

}
