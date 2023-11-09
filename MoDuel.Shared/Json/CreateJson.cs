using System.Text.Json.Nodes;

namespace MoDuel.Shared.Json;

/// <summary>
/// Helpers to create json LINQ nodes from different data sources.
/// </summary>
public static class CreateJson {

    /// <summary>
    /// Create a json object from a json file.
    /// </summary>
    public static JsonObject FromFile(string filePath) {
        var node = JsonNode.Parse(File.ReadAllText(filePath));
        if (node is JsonObject obj)
            return obj;
        else
            return [];
    }

}
