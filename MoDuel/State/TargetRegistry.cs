using MoDuel.Serialization;
using MoDuel.Tools;

namespace MoDuel.State;

/// <summary>
/// A container for all the targets within a state.
/// <para>Provides indexing and lookup.</para>
/// </summary>
[SerializeReference]
public class TargetRegistry {

    /// <summary>
    /// The dictionary that will contain all <see cref="Target"/>s.
    /// </summary>
    public readonly Dictionary<int, WeakReference<Target>> Lookup = new();

    /// <summary>
    /// The tool that gives targets unqieu values.
    /// </summary>
    public readonly Indexer Indexer;

    public TargetRegistry(Indexer indexer) {
        Indexer = indexer;
    }

    /// <summary>
    /// Get the next free index from the <see cref="Indexer"/>.
    /// </summary>
    /// <returns>A unique index from the manager.</returns>
    public int GetNextIndex() => Indexer.GetNext();

    /// <summary>
    /// Connects the <see cref="Target.Index"/> to the <see cref="Target"/> inside the <see cref="Lookup"/>.
    /// </summary>
    /// <returns>False if the <see cref="Target.Index"/> was already present.</returns>
    public bool Assign(Target target) {
        if (Lookup.ContainsKey(target.Index))
            return false;
        Lookup.Add(target.Index, new(target));
        return true;
    }

    /// <summary>
    /// Removes the provided <paramref name="target"/> from the <see cref="Lookup"/> and it's <see cref="Target.Index"/> from the <see cref="Indexer"/>.
    /// <para>Catches all error to esnure deconstructor safety.</para>
    /// </summary>
    public void FreeTarget(Target target) {
        try {
            Lookup.Remove(target.Index);
            Indexer.FreeIndex(target.Index);
        }
        catch { }
    }

    /// <summary>
    /// Returns the <see cref="Lookup"/> value of the <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">The type of target to look for.</typeparam>
    /// <param name="index">The <see cref="Target.Index"/> of the <see cref="Target"/>.</param>
    /// <returns>The requested <see cref="Target"/> or null if it's <paramref name="index"/> was not in the <see cref="Lookup"/> or was not type <typeparamref name="T"/>..</returns>
    public T? GetTarget<T>(int index) where T : Target {
        if (Lookup.TryGetValue(index, out var value)) {
            if (value.TryGetTarget(out var target)) {
                if (target is T result) {
                    return result;
                }
                if (target == null) {
                    return null;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the <see cref="Lookup"/> value of the <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The <see cref="Target.Index"/> of the <see cref="Target"/>.</param>
    /// <returns>The requested <see cref="Target"/> or null if it's <paramref name="index"/> was not in the <see cref="Lookup"/>.</returns>
    public Target? GetTarget(int index) {
        return GetTarget<Target>(index);
    }

}
