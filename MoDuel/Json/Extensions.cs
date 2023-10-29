using Newtonsoft.Json.Linq;
using System.Reflection;

namespace MoDuel.Json;

/// <summary>
/// Extensions for you with Json.Net's json linq objects.
/// </summary>
public static class Extensions {

    /// <summary>
    /// Checks wether the token is the root node of the json data.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static bool IsRoot(this JToken token) {
        return token.Root == token;
    }

    /// <summary>
    /// Checks to see if this <paramref name="token"/> is the <see cref="DeadToken.Instance"/>.
    /// </summary>
    public static bool IsDead(this JToken token) {
        return token == DeadToken.Instance;
    }

    /// <summary>
    /// Returns the refernce or a <see cref="DeadToken.Instance"/> whenn used and the refernce is null.
    /// </summary>
    public static JToken ConvertNullToDeadToken(this JToken? tokenMaybe) {
        return tokenMaybe ?? DeadToken.Instance;
    }

    /// <summary>
    /// Returns the reference or an empty <see cref="JObject"/> when used and the refernce is null.
    /// </summary>
    public static JObject ConvertNullToEmptyToken(this JToken? tokenMaybe) {
        if (tokenMaybe is JObject obj)
            return obj;
        else
            return new JObject();
    }

    /// <summary>
    /// Get the highest index found within the token that is valid for use in <see cref="TryGet(JToken, int)"/>
    /// </summary>
    /// <returns>The highest valid index or null if one could not be found.</returns>
    public static int? GetHighestIndex(this JToken token) {

        if (token is JArray array) {
            return array.Count - 1;
        }
        else if (token is JObject obj) {
            int? currentHighest = null;
            // Search for properties that have names that are valid integers.
            foreach (JProperty property in obj.OfType<JProperty>()) {
                if (int.TryParse(property.Name, out var result)) {
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
    /// Gets all the property values that can be read as ints as a sorted set of that int.
    /// </summary>
    /// <param name="skipNegative">Should negative indicies be skiped rather than included.</param>
    public static SortedSet<int> GetIndicies(this JObject obj, bool skipNegative = true) {

        SortedSet<int> result = new();

        // Add each key value that can be parsed as an int.
        foreach (var prop in obj.Properties()) {
            if (int.TryParse(prop.Name, out var index)) {
                if (index >= 0 || skipNegative)
                    result.Add(index);
            }
        }
        return result;
    }


    /// <summary>
    /// Get the lowest index found within the token that is valid for use in <see cref="TryGet(JToken, int)"/>.
    /// </summary>
    /// <returns>The lowest valid index or null if one could not be found.</returns>
    public static int? GetLowestIndex(this JToken token) {
        if (token is JArray) {
            return 0;
        }
        else if (token is JObject obj) {
            int? currentLowest = null;
            // Search for properties that have names that are valid integers.
            foreach (JProperty property in obj.OfType<JProperty>()) {
                if (int.TryParse(property.Name, out var result)) {
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
    /// <para>For <see cref="JObject"/>s also attempts to use index as a string accessor.</para>
    /// </summary>
    /// <returns>The value forund or <see cref="DeadToken.Instance"/></returns>
    public static JToken TryGet(this JToken token, int index) {
        if (token is JValue)
            return DeadToken.Instance;
        if (token is JArray array) {
            if (array.Count > index) {
                return array[index];
            }
        }
        else if (token is JObject) {
            return token[index.ToString()] ?? DeadToken.Instance;
        }
        return DeadToken.Instance;
    }


    /// <summary>
    /// Tries to get the element with <paramref name="key"/>.
    /// </summary>
    /// <returns>The value forund or <see cref="DeadToken.Instance"/></returns>
    public static JToken TryGet(this JToken token, string key) {
        if (token is not JObject)
            return DeadToken.Instance;
        var value = token[key];
        return value ?? DeadToken.Instance;
    }

    /// <summary>
    /// Attempts to use <see cref="TryGet(JToken, int)"/> but if no value is returned uses the highest index and tries again.
    /// </summary>
    public static JToken TryGetOrHighest(this JToken token, int index) {
        var value = token.TryGet(index);
        if (!value.IsDead()) {
            return value;
        }
        int? highest = token.GetHighestIndex();
        if (highest.HasValue) {
            value = token.TryGet(highest.Value);
        }
        return value;
    }

    /// <summary>
    /// Attempts to use <see cref="TryGet(JToken, int)"/> but if no value is returned uses the next highest index and tries again.
    /// </summary>
    /// <param name="fallbackToHighest">If there are no values greater than <paramref name="index"/> fallback to the highest index in the <paramref name="token"/>.</param>
    public static JToken TryGetOrNextHighest(this JToken token, int index, bool fallbackToHighest = true) {
        var value = token.TryGet(index);
        if (!value.IsDead()) return value;
        if (token is JArray && fallbackToHighest) {
            int? highest = token.GetHighestIndex();
            if (highest.HasValue) {
                return token.TryGet(highest.Value);
            }
        }
        // Try to get the next highest index.
        if (token is JObject obj) {
            var indicies = GetIndicies(obj);
            index = indicies.FirstOrDefault((i) => i > index, -1);
            if (index != -1) return TryGet(obj, index);
            if (fallbackToHighest) {
                int? highest = token.GetHighestIndex();
                if (highest.HasValue) {
                    return token.TryGet(highest.Value);
                }
            }
        }

        return value;
    }

    /// <summary>
    /// Takes an index an if the <paramref name="token"/> is a array coneverts it to base 0 from base 1.
    /// </summary>
    public static int GetAdaptiveIndex(this JToken token, int index) => token is JArray ? index - 1 : index;

    /// <summary>
    /// Attempts to use <see cref="TryGetOrHighest(JToken, int)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    public static JToken BaseFallbackGetOrHighest(this JToken token, int index) {
        var result = TryGetOrHighest(token, index);
        return result.IsDead() ? token : result;
    }


    /// <summary>
    /// Attempts to use <see cref="TryGetOrNextHighest(JToken, int)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    /// <param name="fallbackToHighest">If there are no values greater than <paramref name="index"/> fallback to the highest index in the <paramref name="token"/>.</param>
    public static JToken BaseFallbackGetOrNextHighest(this JToken token, int index, bool fallbackToHighest = true) {
        var result = TryGetOrNextHighest(token, index, fallbackToHighest);
        return result.IsDead() ? token : result;
    }

    /// <summary>
    /// Attempts to use <see cref="TryGet(JToken, int)"/> but if no value is returned uses the lowest index and tries again.
    /// </summary>
    public static JToken TryGetOrLowest(this JToken token, int index) {
        var value = token.TryGet(index);
        if (!value.IsDead()) {
            return value;
        }
        int? lowest = token.GetLowestIndex();
        if (lowest.HasValue) {
            value = token.TryGet(lowest.Value);
        }
        return value;
    }

    /// <summary>
    /// Attempts to use <see cref="TryGet(JToken, int)"/> but if no value is returned uses the next lowest index and tries again.
    /// </summary>
    /// <param name="fallbackToLowest">If there are no values smaller than <paramref name="index"/> fallback to the lowest index in the <paramref name="token"/>.</param>
    public static JToken TryGetOrNextLowest(this JToken token, int index, bool fallbackToLowest = true) {
        var value = token.TryGet(index);
        if (!value.IsDead()) return value;
        if (token is JArray array) {
            // If the index is above the length of array uses the highest index of the array.
            if (index >= array.Count) {
                return token.TryGet(array.Count - 1);
            }
        }
        // Try to get the next lowest index.
        if (token is JObject obj) {
            var indicies = GetIndicies(obj);
            index = indicies.Reverse().FirstOrDefault((i) => i < index, -1);
            if (index != -1) return TryGet(obj, index);
            if (fallbackToLowest) {
                int? lowest = token.GetLowestIndex();
                if (lowest.HasValue) {
                    return token.TryGet(lowest.Value);
                }
            }
        }

        return value;
    }

    /// <summary>
    /// Attemps to use <see cref="TryGetOrLowest(JToken, int)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    public static JToken BaseFallbackGetOrLowest(this JToken token, int index) {
        var result = TryGetOrLowest(token, index);
        return result.IsDead() ? token : result;
    }

    /// <summary>
    /// Attemps to use <see cref="TryGetOrNextLowest(JToken, int)"/> but if no value is returned returns the <paramref name="token"/>.
    /// </summary>
    /// <param name="fallbackToLowest">If there are no values smaller than <paramref name="index"/> fallback to the lowest index in the <paramref name="token"/>.</param>
    public static JToken BaseFallbackGetOrNextLowest(this JToken token, int index, bool fallbackToLowest = true) {
        var result = TryGetOrNextLowest(token, index, fallbackToLowest);
        return result.IsDead() ? token : result;
    }



    /// <summary>
    /// Checks to see if the <paramref name="token"/> can and does contain the <paramref name="value"/> provide.
    /// <para>uses each sub-elements <see cref="ToRawValue(JToken)"/> result.</para>
    /// <para>Also returns true if a <see cref="JToken"/> <paramref name="value"/> is a subelement.</para>
    /// </summary>
    public static bool ContainsRawValue(this JToken token, object value, bool caseSensitive = true) {
        if (token is JContainer) {
            foreach (var subToken in token) {

                // Copy the token locally.
                var t = subToken;

                // Check to see if a property value is the value.
                if (subToken is JProperty property) {
                    if (property == value) return true;
                    t = property.Value;
                }

                // Direct match.
                if (t == value) return true;

                // Try to match the raw value.
                var subValue = t.ToRawValue();

                if (!caseSensitive && subValue is string subString && value is string valueString) {
                    if (subString.ToLowerInvariant() == valueString.ToLowerInvariant()) {
                        return true;
                    }
                }
                else if (subValue == value) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the value on the <paramref name="token"/> htat was stored with <see cref="SetReloadItemPath(JToken, string)"/>.
    /// </summary>
    public static string? GetReloadItemPath(this JToken token) {
        if (token is JObject obj) {
            var reload = obj["$reload"];
            return reload?.ToString();
        }
        return null;
    }

    /// <summary>
    /// Sets the <paramref name="itemPath"/> on the token such that it can be reloaded by the serializer.
    /// </summary>
    public static void SetReloadItemPath(this JToken token, string itemPath) {
        if (token is JObject obj) {
            obj["$reload"] = itemPath;
        }
    }

    /// <summary>
    /// Get's the static reload for the token.
    /// <para>This only reloads the <see cref="DeadToken.Instance"/></para>
    /// TODO SERIAL: consider flexibility like with normal reload.
    /// </summary>
    internal static MethodInfo? GetStaticReload(this JToken token) {
        if (token.IsDead()) {
            return typeof(Extensions).GetMethod("GetDeadToken.Instance");
        }
        return null;
    }

    /// <summary>
    /// Sonverts a <see cref="JToken"/> to a jso string so that it can be recreated in <see cref="FromJString(string)"/>.
    /// </summary>
    public static string ToJString(this JToken token) {
        return token.ToString();
    }

    /// <summary>
    /// Reconstructs a <see cref="JToken"/> from the string created in <see cref="ToJString(JToken)"/>.
    /// </summary>
    public static JToken FromJString(string jString) {
        return JToken.Parse(jString);
    }

    /// <summary>
    /// Get's the <see cref="DeadToken.Instance"/>.
    /// <para>THis is for reload purposes.</para>
    /// </summary>
    public static JToken GetDeadToken => DeadToken.Instance;

    /// <summary>
    /// Weap the token in a proxy layer. This is safe to be serialized.
    /// </summary>
    public static JsonProxy ToProxy(this JToken token) => new(token);

    /// <summary>
    /// Converts a <see cref="JValue"/> to the corresponding .NET type.
    /// </summary>
    public static dynamic? ToRawValue(this JToken token) => token.Type switch {
        JTokenType.Integer => (int?)token,
        JTokenType.Float => (float?)token,
        JTokenType.String => (string?)token,
        JTokenType.Boolean => (bool?)token,
        _ => null,
    };

    /// <summary>
    /// Tries to convert the <see cref="JToken"/> to it's corresponding .NET type.
    /// <para>If there is no valid value it will instead return <paramref name="fallback"/>.</para>
    /// </summary>
    public static dynamic? ToRawValueOrFallback(this JToken token, object fallback) {
        var result = ToRawValue(token);
        if (result == null) {
            return fallback;
        }
        return result;
    }

}
