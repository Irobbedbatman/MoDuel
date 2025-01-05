using System.Collections;

namespace MoDuel.Shared;

/// <summary>
/// A value that can be marked with multiple sources. Useful if a value should remain while there is any source but not remain when all sources are removed.
/// </summary>
public class SourcedValue<T>(T? value, params object[] initialSources) : IEquatable<SourcedValue<T?>>, IEquatable<T?>, IComparable<T>, IComparable<SourcedValue<T>>, ICollection<object> {

    /// <summary>
    /// The set of objects that define the value to be as such.
    /// </summary>
    public readonly HashSet<object> Sources = [.. initialSources];

    /// <summary>
    /// The raw value contained.
    /// </summary>
    public readonly T? Value = value;

    /// <summary>
    /// Does this value have a source. If it doesn't it should not be used.
    /// </summary>
    public bool Valid => Sources.Count != 0;

    /// <inheritdoc/>
    public int Count => Sources.Count;

    /// <summary>
    /// This collection is never read only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Add a new source to the value.
    /// </summary>
    public void Add(object source) => Sources.Add(source);

    /// <summary>
    /// Check to see if two value sources contain the same value.
    /// </summary>
    public bool Equals(SourcedValue<T?>? other) => other != null && Equals(other.Value);

    /// <summary>
    /// Remove a current source from the value.
    /// </summary>
    public bool Remove(object source) => Sources.Remove(source);

    /// <summary>
    /// Check to see if <see cref="Value"/> equates to the provided <paramref name="obj"/>.
    /// </summary>
    public override bool Equals(object? obj) {
        if (obj is SourcedValue<T> other)
            return Equals(other.Value);
        if (obj is T t)
            return Equals(t);
        return false;
    }

    /// <summary>
    /// The hash code of the contained <see cref="Value"/>.
    /// </summary>
    public override int GetHashCode() => Value?.GetHashCode() ?? -1;

    /// <summary>
    /// Check to see if <see cref="Value"/> equates to the provided <paramref name="other"/> value.
    /// </summary>
    public bool Equals(T? other) => Value?.Equals(other) ?? other == null;

    /// <summary>
    /// Compare the <see cref="Value"/> to the provided <paramref name="other"/> value.
    /// </summary>
    public int CompareTo(T? other) => Value is IComparable vic && other is IComparable oic ? vic.CompareTo(oic) : 0;

    /// <summary>
    /// Compare the <see cref="Value"/> to the provided <paramref name="other"/> source's value.
    /// </summary>

    public int CompareTo(SourcedValue<T>? other) => other == null ? 0 : CompareTo(other.Value);

    /// <summary>
    /// Remove all sources.
    /// </summary>
    public void Clear() => Sources.Clear();

    /// <summary>
    /// Check to see a source exists.
    /// </summary>
    public bool Contains(object item) => Sources.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(object[] array, int arrayIndex) => Sources.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<object> GetEnumerator() => Sources.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => Sources.GetEnumerator();

    /// <summary>
    /// Convert the source value to it's contained value.
    /// </summary>
    /// <param name="sourceValue"></param>
    public static implicit operator T?(SourcedValue<T?> sourceValue) => sourceValue.Value;

}
