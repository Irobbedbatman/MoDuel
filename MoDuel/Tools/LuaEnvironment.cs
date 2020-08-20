using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace MoDuel.Tools {

    /// <summary>
    /// A singleton wrapper around a <see cref="Script"/>.
    /// </summary>
    public class LuaEnvironment {

        private static readonly LuaEnvironment _instance = new LuaEnvironment();

        private Script _moonSharpScript = new Script(CoreModules.Preset_HardSandbox);

        public static Action<string> Output;

        private LuaEnvironment() {
            JArrayProxy.Register();
            _moonSharpScript.Options.DebugPrint = s => { Output?.Invoke(s); };
        }

        public static implicit operator Script (LuaEnvironment luaRunner) {
            return luaRunner._moonSharpScript;
        }

        public static Script Instance => _instance;


    }
}
