namespace MoDuel.Shared.Structures;

/// <summary>
/// A list with added helper methods.
/// </summary>
public class DataSet<T> : List<T>, ICloneable {

    public DataSet() { }

    public DataSet(IEnumerable<T> items) {
        AddRange(items);
    }

    /// <summary>
    /// Create a shallow clone of the data table.
    /// </summary>
    public virtual DataSet<T> Clone() => Clone();

    /// <inheritdoc/>
    object ICloneable.Clone() => new DataSet<T>(this);
}
