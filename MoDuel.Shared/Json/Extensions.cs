using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MoDuel.Json;

/// <summary>
/// Extensions for json linq objects.
/// </summary>
public static class Extensions {

    /// <summary>
    /// Checks whether the token is the root node of the json data.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static bool IsRoot(this JsonNode token) {
        return token.Root == token;
    }

    /// <summary>
    /// Retrieve the json node after traversing the bath. If the path is invalid the <see cref="DeadToken.Instance"/> will be returned instead.
    /// </summary>
    public static JsonNode GetUsingPath(this JsonNode token, string path) {
        // TODOD: GetAtPath
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks to see if this <paramref name="token"/> is the <see cref="DeadToken.Instance"/>.
    /// </summary>
    public static bool IsDead(this JsonNode token) {
        return token == DeadToken.Instance;
    }

    /// <summary>
    /// Returns the reference or a <see cref="DeadToken.Instance"/> when used and the reference is null.
    /// </summary>
    public static JsonNode ConvertNullToDeadToken(this JsonNode? tokenMaybe) {
        return tokenMaybe ?? DeadToken.Instance;
    }

    /// <summary>
    /// Returns the reference or an empty <see cref="JsonObject"/> when used and the reference is null.
    /// </summary>
    public static JsonObject ConvertNullToEmptyToken(this JsonNode? tokenMaybe) {
        if (tokenMaybe is JsonObject obj)
            return obj;
        else
            return [];
    }

    /// <summary>
    /// Get the highest index found within the token that is valid for use in <see cref="Get(JsonNode, int)"/>
    /// </summary>
    /// <returns>The highest valid index or null if one could not be found.</returns>
    public static int? GetHighestIndex(this JsonNode token) {

        if (token is JsonArray array) {
            return array.Count - 1;
        }
        else if (token is JsonObject obj) {
            int? currentHighest = null;
            // Search for properties that have names that are valid integers.
            foreach (var property in obj) {
                if (int.TryParse(property.Key, out var result)) {
                    // If a valid value is found and none were found prior use it.
                    if (currentHighest == null)
                        currentHighest = result;
                    else if (result > currentHighest)
                        currentHighest = result;
                }
            }
            return currentHighest;
        }
        return null;
    }

    /// <summary>
    /// Gets all the property values that can be read as integers as a sorted set of that int.
    /// </summary>
    /// <param name="skipNegative">Should negative indices be skied rather than included.</param>
    public static SortedSet<int> GetIndices(this JsonObject obj, bool skipNegative = true) {

        SortedSet<int> result = [];

        // Add each key value that can be parsed as an int.
        foreach (var prop in obj) {
            if (int.TryParse(prop.Key, out var index)) {
                if (index >= 0 || skipNegative)
                    result.Add(index);
            }
        }
        return result;
    }


    /// <summary>
    /// Get the lowest index found within the token that is valid for use in <see cref="Get(JsonNode, int)"/>.
    /// </summary>
    /// <returns>The lowest valid index or null if one could not be found.</returns>
    public static int? GetLowestIndex(this JsonNode token) {
        if (token is JsonArray) {
            return 0;
        }
        else if (token is JsonObject obj) {
            int? currentLowest = null;
            // Search for properties that have names that are valid integers.
            foreach (var property in obj) {
                if (int.TryParse(property.Key, out var result)) {
                    // If a valid value is found and none were found prior use it.
                    if (currentLowest == null)
                        currentLowest = result;
                    else if (result < currentLowest)
                        currentLowest = result;
                }
            }
            return currentLowest;
        }
        return null;
    }

    /// <summary>
    /// Tries to get the element at <paramref name="index"/>.
    /// <para>For <see cref="JsonObject"/>s also attempts to use index as a string accessor.</para>
    /// </summary>
    /// <returns>The value found or <see cref="DeadToken.Instance"/></returns>
    public static JsonNode Get(this JsonNode token, int index) {
        if (token is JsonValue)
            return DeadToken.Instance;
        if (token is JsonArray array) {
            if (array.Count > index) {
                return array[index] ?? DeadToken.Instance;
            }
        }
        else if (token is JsonObject) {
            return token[index.ToString()] ?? DeadToken.Instance;
        }
        return DeadToken.Instance;
    }

    /// <summary>
    /// Tries to get the element with <paramref name="key"/>.
    /// </summary>
    /// <returns>The value found or <see cref="DeadToken.Instance"/></returns>
    public static JsonNode Get(this JsonNode token, string key) {
        if (token is not JsonObject)
            return DeadToken.Instance;
        var value = token[key];
        return value ?? DeadToken.Instance;
    }

    /// <summary>
    /// Tries to get the element with <paramref name="key"/>.
    /// <para>If the element can be found returns true.</para>
    /// <para>If the element could not be found returns false and the result will be a <see cref="DeadToken.Instance"/>.</para>
    /// </summary>
    public static bool TryGet(this JsonNode token, string key, out JsonNode result) {
        result = Get(token, key);
        return result != DeadToken.Instance;
    }

    /// <summary>
    /// Tries to get the element with <paramref name="key"/>.
    /// <para>If the element can be found returns true.</para>
    /// <para>If the element could not be found returns false and the result will be a <see cref="DeadToken.Instance"/>.</para>
    /// </summary>
    public static bool TryGet(this JsonNode token, int key, out JsonNode result) {
        result = Get(token, key);
        return result != DeadToken.Instance;
    }

    /// <summary>
    /// Attempts to use <see cref="Get(JsonNode, int)"/> but if no value is returned uses the highest index and tries again.
    /// </summary>
    public static JsonNode GetOrHighest(this JsonNode token, int index) {
        var value = token.Get(index);
        if (!value.IsDead()) {
            return value;
        }
        int? highest = token.GetHighestIndex();
        if (highest.HasValue) {
            value = token.Get(highest.Value);
        }
        return value;
    }

    /// <summary>
    /// Attempts to use <see cref="Get(JsonNode, int)"/> but if no value is returned uses the next highest index and tries again.
    /// </summary>
    /// <param name="fallbackToHighest">If there are no values greater than <paramref name="index"/> fallback to the highest index in the <paramref name="token"/>.</param>
    public static JsonNode GetOrNextHighest(this JsonNode token, int index, bool fallbackToHighest = true) {
        var value = token.Get(index);
        if (!value.IsDead()) return value;
        if (token is JsonArray && fallbackToHighest) {
            int? highest = token.GetHighestIndex();
            if (highest.HasValue) {
                return token.Get(highest.Value);
            }
        }
        // Try to get the next highest index.
        if (token is JsonObject obj) {
            var indices = GetIndices(obj);
            index = indices.FirstOrDefault((i) => i > index, -1);
            if (index != -1) return Get(obj, index);
            if (fallbackToHighest) {
                int? highest = token.GetHighestIndex();
                if (highest.HasValue) {
                    return token.Get(highest.Value);
                }
            }
        }

        return value;
    }

    /// <summary>
    /// Takes an index an if the <paramref name="token"/> is a array converts it to base 0 from base 1.
    /// </summary>
    public static int GetAdaptiveIndex(this JsonNode token, int index) => token is JsonArray ? index - 1 : index;

    /// <summary>
    /// Attempts to use <see cref="GetOrHighest(JsonNode, int)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    public static JsonNode BaseFallbackGetOrHighest(this JsonNode token, int index) {
        var result = GetOrHighest(token, index);
        return result.IsDead() ? token : result;
    }


    /// <summary>
    /// Attempts to use <see cref="GetOrNextHighest(JsonNode, int, bool)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    /// <param name="fallbackToHighest">If there are no values greater than <paramref name="index"/> fallback to the highest index in the <paramref name="token"/>.</param>
    public static JsonNode BaseFallbackGetOrNextHighest(this JsonNode token, int index, bool fallbackToHighest = true) {
        var result = GetOrNextHighest(token, index, fallbackToHighest);
        return result.IsDead() ? token : result;
    }

    /// <summary>
    /// Attempts to use <see cref="Get(JsonNode, int)"/> but if no value is returned uses the lowest index and tries again.
    /// </summary>
    public static JsonNode GetOrLowest(this JsonNode token, int index) {
        var value = token.Get(index);
        if (!value.IsDead()) {
            return value;
        }
        int? lowest = token.GetLowestIndex();
        if (lowest.HasValue) {
            value = token.Get(lowest.Value);
        }
        return value;
    }

    /// <summary>
    /// Attempts to use <see cref="Get(JsonNode, int)"/> but if no value is returned uses the next lowest index and tries again.
    /// </summary>
    /// <param name="fallbackToLowest">If there are no values smaller than <paramref name="index"/> fallback to the lowest index in the <paramref name="token"/>.</param>
    public static JsonNode GetOrNextLowest(this JsonNode token, int index, bool fallbackToLowest = true) {
        var value = token.Get(index);
        if (!value.IsDead()) return value;
        if (token is JsonArray array) {
            // If the index is above the length of array uses the highest index of the array.
            if (index >= array.Count) {
                return token.Get(array.Count - 1);
            }
        }
        // Try to get the next lowest index.
        if (token is JsonObject obj) {
            var indices = GetIndices(obj);
            index = indices.Reverse().FirstOrDefault((i) => i < index, -1);
            if (index != -1) return Get(obj, index);
            if (fallbackToLowest) {
                int? lowest = token.GetLowestIndex();
                if (lowest.HasValue) {
                    return token.Get(lowest.Value);
                }
            }
        }

        return value;
    }

    /// <summary>
    /// Attempts to use <see cref="GetOrLowest(JsonNode, int)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    public static JsonNode BaseFallbackGetOrLowest(this JsonNode token, int index) {
        var result = GetOrLowest(token, index);
        return result.IsDead() ? token : result;
    }

    /// <summary>
    /// Attempts to use <see cref="GetOrNextLowest(JsonNode, int, bool)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    /// <param name="fallbackToLowest">If there are no values smaller than <paramref name="index"/> fallback to the lowest index in the <paramref name="token"/>.</param>
    public static JsonNode BaseFallbackGetOrNextLowest(this JsonNode token, int index, bool fallbackToLowest = true) {
        var result = GetOrNextLowest(token, index, fallbackToLowest);
        return result.IsDead() ? token : result;
    }

    /// <summary>
    /// Checks to see if the <paramref name="token"/> can and does contain the <paramref name="value"/> provide.
    /// <para>uses each sub-elements <see cref="ToRawValue(JsonNode)"/> result.</para>
    /// <para>Also returns true if a <see cref="JsonNode"/> <paramref name="value"/> is a sub element.</para>
    /// </summary>
    public static bool ContainsRawValue(this JsonNode token, object value, bool caseSensitive = true) {
        foreach (var subToken in token.GetValues()) {

            // Copy the token locally.
            JsonNode? t = subToken;

            // Direct match.
            if (t == value) return true;

            // Try to match the raw value.
            var subValue = t?.ToRawValue();

            if (!caseSensitive && subValue is string subString && value is string valueString) {
                if (subString.Equals(valueString, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }
            else if (subValue == value) return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the value on the <paramref name="token"/> that was stored with <see cref="SetReloadItemPath(JsonNode, string)"/>.
    /// </summary>
    public static string? GetReloadItemPath(this JsonNode token) {
        if (token is JsonObject obj) {
            var reload = obj["$reload"];
            return reload?.ToString();
        }
        return null;
    }

    /// <summary>
    /// Sets the <paramref name="itemPath"/> on the token such that it can be reloaded by the serializer.
    /// </summary>
    public static void SetReloadItemPath(this JsonNode token, string itemPath) {
        if (token is JsonObject obj) {
            obj["$reload"] = itemPath;
        }
    }

    /// <summary>
    /// Get's the static reload for the token.
    /// <para>This only reloads the <see cref="DeadToken.Instance"/></para>
    /// TODO SERIAL: consider flexibility like with normal reload.
    /// </summary>
    internal static MethodInfo? GetStaticReload(this JsonNode token) {
        if (token.IsDead()) {
            return typeof(Extensions).GetMethod("GetDeadToken");
        }
        return null;
    }

    /// <summary>
    /// Converts a <see cref="JsonNode"/> to a json string so that it can be recreated in <see cref="FromJString(string)"/>.
    /// </summary>
    public static string ToJString(this JsonNode token) {
        return token.ToString();
    }

    /// <summary>
    /// Reconstructs a <see cref="JsonNode"/> from the string created in <see cref="ToJString(JsonNode)"/>.
    /// </summary>
    public static JsonNode FromJString(string jString) {
        return JsonNode.Parse(jString) ?? DeadToken.Instance;
    }

    /// <summary>
    /// Get's the <see cref="DeadToken.Instance"/>.
    /// <para>THis is for reload purposes.</para>
    /// </summary>
    public static JsonNode GetDeadToken => DeadToken.Instance;

    /// <summary>
    /// Converts a <see cref="JsonValue"/> to the corresponding .NET type.
    /// </summary>
    public static object? ToRawValue(this JsonNode token) {
        if (token == DeadToken.Instance) return null;
        return token.GetValueKind() switch {
            JsonValueKind.String => (string?)token,
            JsonValueKind.Number => (double?)token,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }


    /// <summary>
    /// Converts a <see cref="JsonValue"/> to the generic type.
    /// <para>The only values that work are double, string and bool.</para>
    /// </summary>
    public static T? ToRawValue<T>(this JsonNode token) {
        var value = ToRawValue(token);
        if (value is T t) {
            return t;
        }
        // TOOD: Convert using IConvertible interface
        try {
            return (T?)Convert.ChangeType(value, typeof(T));
        }
        catch { }
        return default;
    }

    /// <summary>
    /// Tries to convert the <see cref="JsonNode"/> to it's corresponding .NET type.
    /// <para>If there is no valid value it will instead return <paramref name="fallback"/>.</para>
    /// </summary>
    [return: NotNullIfNotNull(nameof(fallback))]
    public static object? ToRawValueOrFallback(this JsonNode token, object fallback) {
        var result = ToRawValue(token);
        if (result == null) {
            return fallback;
        }
        return result;
    }


    /// <summary>
    /// Tries to convert the <see cref="JsonNode"/> to the provided generic type.
    /// <para>If there is no valid value it will instead return <paramref name="fallback"/>.</para>
    /// </summary>
    [return: NotNullIfNotNull(nameof(fallback))]
    public static T? ToRawValueOrFallback<T>(this JsonNode token, T? fallback) {
        if (token == DeadToken.Instance) return fallback;
        object? result = token.GetValueKind() switch {
            JsonValueKind.String => (string?)token,
            JsonValueKind.Number => (double?)token,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => fallback,
            JsonValueKind.Object => fallback,
            JsonValueKind.Array => fallback,
            _ => fallback
        };
        if (result == null) return fallback;
        if (result is T t) return t;
        // TOOD: Convert using IConvertible interface
        try {
            return (T?)Convert.ChangeType(result, typeof(T));
        }
        catch { }
        return fallback;
    }

    /// <summary>
    /// Is the <see cref="JsonNode"/> able to contain other nodes.
    /// </summary>
    public static bool IsCollection(this JsonNode node) {
        if (node is JsonArray or JsonObject)
            return true;
        return false;
    }

    /// <summary>
    /// Get the <see cref="JsonNode"/> as an enumerable of it's sub values. For <see cref="JsonObject"/> the value of each property will be returned.
    /// <para>Returns an empty set if the <paramref name="node"/> cannot hold other values.</para>
    /// </summary>
    public static IEnumerable<JsonNode> GetValues(this JsonNode node) {
        if (node is JsonObject obj) return obj.AsEnumerable().Select((pair) => pair.Value ?? DeadToken.Instance);
        if (node is JsonArray arr) return arr.AsEnumerable().Select((item) => item ?? DeadToken.Instance);
        return [];
    }

    /// <summary>
    /// Get all the key of any sub values within the node.
    /// </summary>
    public static IEnumerable<string> GetKeys(this JsonNode node) {
        if (node is JsonObject obj)
            return obj.Select(pair => pair.Key);
        else return [];
    }

    /// <summary>   
    /// Gets all the properties from a json node. This is an empty set for non-object json data.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, JsonNode>> GetProperties(this JsonNode node) {
        if (node is JsonObject obj)
            return obj.Select(pair => new KeyValuePair<string, JsonNode>(pair.Key, pair.Value ?? DeadToken.Instance));
        else return [];
    }

}
