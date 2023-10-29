using MoDuel.Serialization;

namespace MoDuel.Tools;

/// <summary>
/// A class for handling unqiue indexed values.
/// </summary>
[SerializeReference]
public class Indexer {

    /// <summary>
    /// The index to use when not assigned an index.
    /// </summary>
    public const int UNSET_INDEX = -1;

    /// <summary>
    /// The highest index value that has been created.
    /// </summary>
    private int _highestIndex = 0;

    /// <summary>
    /// A stack that contains all the freed indicies for resuse.
    /// </summary>
    private readonly Stack<int> _freeValues = new();

    /// <summary>
    /// Gets the next unused index.
    /// <para>If there is a freed index we use that first.</para>
    /// </summary>
    /// <returns>A unqiue index.</returns>
    public int GetNext() {
        return _freeValues.TryPop(out int top) ? top : _highestIndex++;
    }

    /// <summary>
    /// Frees an index so that it can be used again.
    /// </summary>
    /// <param name="value">The index to free.</param>
    public void FreeIndex(int index) {
        _freeValues.Push(index);
    }

}