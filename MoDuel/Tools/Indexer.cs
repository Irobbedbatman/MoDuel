using System.Collections.Generic;

namespace MoDuel.Tools {
    
    /// <summary>
    /// A class for handling unqiue indexed values.
    /// </summary>
    public class Indexer {

        /// <summary>
        /// The highest index value that has been created.
        /// </summary>
        private int _highestIndex = 0;

        /// <summary>
        /// A stack that contains all the freed indicies for resuse.
        /// </summary>
        private readonly static Stack<int> _freeValues = new Stack<int>();

        /// <summary>
        /// Gets the next unused index.
        /// <para>If there is a freed index we use that first.</para>
        /// </summary>
        /// <returns>A unqiue index.</returns>
        public int GetNext() {
            if (_freeValues.Count > 0)
                return _freeValues.Pop();
            else
                return _highestIndex++;
        }

        /// <summary>
        /// Frees an index so that it can be used again.
        /// </summary>
        /// <param name="value">The index to free.</param>
        public void Free(int value) {
            _freeValues.Push(value);
        }
    }
}
