using MoDuel.Data;
using MoonSharp.Environment;

namespace MoDuel.Tools {

    /// <summary>
    /// Container for the lua interpreter and loaded content.
    /// <para>Individual instances of <see cref="DuelFlow"/> should have a diffrent <see cref="LuaEnvironment"/> but can share their <see cref="LoadedContent"/></para>
    /// </summary>
    public struct EnvironmentContainer {

        public LuaEnvironment Lua;
        public LoadedContent Content;

        public EnvironmentContainer(LuaEnvironment luaEngine, LoadedContent contentHandler) {
            Lua = luaEngine;
            Content = contentHandler;
        }

    }
}
