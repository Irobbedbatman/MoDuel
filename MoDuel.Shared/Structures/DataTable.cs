namespace MoDuel.Shared.Structures;

/// <summary>
/// A managed table for use in overwrite triggers. It is effectively a string to object dictionary.
/// </summary>
public class DataTable : Dictionary<string, object?> {

    public DataTable() { }

    /// <summary>
    /// Create a new data table using a shallow clone.
    /// </summary>
    public DataTable(Dictionary<string, object?> values) {
        foreach (var pair in values) {
            Add(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Tries to get the value with the name <paramref name="key"/> of the type <typeparamref name="T"/>.
    /// <para>Returns default if the value was not found or is the wrong type.</para>
    /// </summary>
    public T? Get<T>(string key) {
        var value = this.GetValueOrDefault(key, null);
        if (value is T tValue) {
            return tValue;
        }
        return default;
    }

    /// <summary>
    /// Tries to get the value with the name <paramref name="key"/> of the type <typeparamref name="T"/>.
    /// <para>Inserts the value if none was found.</para>
    /// </summary>
    public T? GetOrAdd<T>(string key, T? newValue) {
        if (TryGetValue(key, out var value)) {
            if (value is T tValue) return tValue;
            return default;
        }
        Add(key, newValue);
        return newValue;
    }

    /// <summary>
    /// Create a shallow clone of the data table.
    /// </summary>
    public virtual T Clone<T>() where T : DataTable {
        var result = Activator.CreateInstance<T>();
        foreach (var pair in this) {
            result.Add(pair.Key, pair.Value);
        }
        return result;
    }

}
