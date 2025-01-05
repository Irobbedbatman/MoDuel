namespace MoDuel.Shared.Structures;

/// <summary>
/// A specific version fo a <see cref="DataTable"/> that is used to get an explicit result.
/// <para>THe result is recorded under the key 'Result'.</para>
/// </summary>
/// <typeparam name="T">The type of the result to retrieve.</typeparam>
public class ResultDataTable<T> : DataTable {

    public ResultDataTable() { }

    public ResultDataTable(T? initialResult) {
        this["Result"] = initialResult;
    }

    /// <summary>
    /// Create a new result data table using a shallow clone.
    /// </summary>
    public ResultDataTable(Dictionary<string, object?> values) : base(values) { }

    /// <summary>
    /// Indirect way to get the result from the data table.
    /// </summary>
    public T? GetResult() => Get<T>("Result");

    /// <summary>
    /// Convertor for results data tables to the result stored.
    /// </summary>
    public static implicit operator T?(ResultDataTable<T> dataTable) {
        return dataTable.GetResult();
    }

}
