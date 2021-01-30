using MoDuel.Data;
using MoonSharp.Environment;

namespace MoDuel {

    /// <summary>
    /// Container for the lua interpreter and loaded content.
    /// <para>Individual instances of <see cref="DuelFlow"/> should have a diffrent <see cref="LuaEnvironment"/> but can share their <see cref="LoadedContent"/></para>
    /// </summary>
    public struct EnvironmentContainer {

        public DuelSettings Settings;
        public LuaEnvironment Lua;
        public LoadedContent Content;
        public Tools.ManagedRandom Random;


        public EnvironmentContainer(DuelSettings settings, LuaEnvironment luaEngine, LoadedContent contentHandler, Tools.ManagedRandom random) {
            Lua = luaEngine;
            Content = contentHandler;
            Settings = settings;
            Random = random;
        }

    }
}
