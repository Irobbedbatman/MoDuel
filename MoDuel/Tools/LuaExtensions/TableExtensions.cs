using MoonSharp.Interpreter;

namespace MoDuel.Tools.LuaExtensions {
    public static class TableExtensions {

        /// <summary>
        /// Combines two tables by adding all of <paramref name="addedTable"/> to <paramref name="table"/>.
        /// </summary>
        /// <param name="table">The table to be added to.</param>
        /// <param name="addedTable">The table to be added from.</param>
        public static void AddTable(this Table table, Table addedTable) {
            foreach (var pair in addedTable.Pairs)
                table[pair.Key] = pair.Value;
        }

    }
}
