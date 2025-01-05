namespace MoDuel.Shared.Structures;

/// <summary>
/// Conversions to and from the shared structures.
/// </summary>
public static class Extensions {

    /// <summary>
    /// Convert the list to a new data set.
    /// </summary>
    public static DataSet<T> ToDataSet<T>(this IEnumerable<T> list) => new(list);

    /// <summary>
    /// Convert the list to a new data table.
    /// </summary>
    public static DataTable ToDataTable<T>(this Dictionary<string, T?> dictionary) {
        var data = new DataTable();
        foreach (var pair in dictionary) {
            data.Add(pair.Key, pair.Value);
        }
        return data;
    }

    /// <summary>
    /// Convert the data table to a result data table.
    /// </summary>
    public static ResultDataTable<T> ToResultTable<T>(this DataTable table) {
        return new ResultDataTable<T>(table);
    }

}
