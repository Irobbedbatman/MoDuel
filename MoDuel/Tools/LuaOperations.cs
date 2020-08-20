using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Tools {

    public static class LuaOperations {

        delegate DynValue FuncDoubleRef(ref DynValue refnum);

        public static void AddLuaOps(Script script) {

            script.Globals["Increment"] = (FuncDoubleRef)Increment;

        }

        private static DynValue Increment(ref DynValue num) {
            return num = DynValue.NewNumber(num.Number+1);
        }


        public static Table Add(this Table table, DynValue element) {
            table.Append(element);
            return table;
        }


    }
}
