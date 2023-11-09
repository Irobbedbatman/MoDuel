namespace MoDuel.Triggers;

/// <summary>
/// A managed table for use in overwrite triggers. It is effectively a string to object dictionary.
/// </summary>
public class OverwriteTable : Dictionary<string, object?> {

    public OverwriteTable() { }

    /// <summary>
    /// Tries to get the value with the name <paramref name="key"/>.
    /// <para>Returns default if the value was not found or is the wrong type.</para>
    /// </summary>
    public T? Get<T>(string key) {
        var value = this.GetValueOrDefault(key, null);

        if (value is T tValue) {
            return tValue;
        }

        return default;

    }

}
