using System.Collections.Generic;

namespace MoDuel.Tools {
    /// <summary>
    /// A class for handling unqiue indexed values.
    /// </summary>
    public class Indexer {

        /// <summary>
        /// The highest value recorded.
        /// </summary>
        private int _MaximumValue = 0;

        /// <summary>
        /// A stack to use any index that has been freed up first.
        /// </summary>
        private static Stack<int> _FreeValues = new Stack<int>();

        /// <summary>
        /// Gets the next unused index.
        /// </summary>
        /// <returns>A unqiue index.</returns>
        public int GetNext() {
            if (_FreeValues.Count > 0)
                return _FreeValues.Pop();
            else
                return _MaximumValue++;
        }

        /// <summary>
        /// Frees an index so that it can be used again.
        /// </summary>
        /// <param name="value">The index to free.</param>
        public void Free(int value) {
            _FreeValues.Push(value);
        }
    }
}
