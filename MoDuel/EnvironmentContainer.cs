using MoDuel.Animation;
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
        /// <summary>
        /// The contoller of animation blocking.
        /// </summary>
        public  AnimationBlockingHandler AnimationBlocker;

        public EnvironmentContainer(DuelSettings settings, LuaEnvironment luaEngine, LoadedContent contentHandler, Tools.ManagedRandom random, AnimationBlockingHandler animationBlocker) {
            Lua = luaEngine;
            Content = contentHandler;
            Settings = settings;
            Random = random;
            AnimationBlocker = animationBlocker;
        }

    }
}
