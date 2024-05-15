using System.Collections;

namespace MoDuel;

/// <summary>
/// A collection that records multiple <see cref="SourcedValue{T}"/>s and provides set utility alongside specialized utility to handle the values.
/// </summary>
/// <typeparam name="T">The internal value that is stored.</typeparam>
public class SourcedValueCollection<T> : IEnumerable<T?>, ICollection {

    /// <summary>
    /// The <see cref="SourcedValue{T}"/>s stored in the set. Sourced values are equivalent if they have the same internal value.
    /// <para>Stored as objects as they need to be compared at the object level.</para>
    /// </summary>
    private readonly HashSet<object?> Values = [];

    /// <inheritdoc/>
    public int Count => Values.Count;

    /// <inheritdoc/>
    public bool IsSynchronized => false;

    /// <inheritdoc/>
    public object SyncRoot => new();

    /// <summary>
    /// Adds a <paramref name="source"/> to the collection an ties the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source that added the value.</param>
    /// <param name="value">The value to add to the collection.</param>
    public void Add(object source, T? value) {

        // If the value has already been added; tie the source to he pre-existing value.
        if (Values.TryGetValue(value, out var actualValue)) {
            if (actualValue is SourcedValue<T> sourceValue) {
                sourceValue.Add(source);
            }
            return;
        }
        Values.Add(new SourcedValue<T>(value, source));

    }

    /// <summary>
    /// Retrieves the value with the most sources. If no value is stored returns the <paramref name="fallback"/> if there is a tie the <paramref name="tieResolver"/> will be used and is provided all values with the most sources.
    /// </summary>
    /// <param name="fallback">The value to return if the collection is empty.</param>
    /// <param name="tieResolver">The function used when multiple values have the highest source.</param>
    public T? GetMostSourcedValue(T? fallback, Func<IEnumerable<T?>, T?> tieResolver) {
        var values = Values.Cast<SourcedValue<T>>().GroupBy(x => x.Count);
        if (!values.Any()) return fallback;
        var highestGroup = values.Last();
        if (highestGroup.Key == 0) return fallback;
        if (!highestGroup.Any()) return fallback;
        if (highestGroup.Count() == 1) return highestGroup.First().Value;
        return tieResolver.Invoke(highestGroup.Select(x => x.Value));
    }

    /// <summary>
    /// Removes a source from a value and if that value no longer contains a source it is removed from the collection.
    /// </summary>
    /// <param name="source">The source that added the value.</param>
    /// <param name="value">The value to remove.</param>
    public void Remove(object source, T? value) {
        if (Values.TryGetValue(value, out var actualValue)) {
            if (actualValue is SourcedValue<T> sourceValue) {
                sourceValue.Remove(source);
                if (!sourceValue.Valid)
                    Values.Remove(value);
            }
        }
    }
    
    /// <summary>
    /// Removes all values from the collection.
    /// </summary>
    public void Clear() => Values.Clear();

    /// <summary>
    /// Checks to see if a value is sourced within the collection.
    /// </summary>
    public bool Contains(T value) => Values.Contains(value);

    /// <inheritdoc/>
    public IEnumerator<T?> GetEnumerator() {
#nullable disable
        return Values.Select(x => ((SourcedValue<T>)x).Value).GetEnumerator();
#nullable enable
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public void CopyTo(Array array, int index) {
        array.SetValue(Values.ToArray()[index], index);
    }
}
