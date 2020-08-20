using System;
using MoonSharp.Interpreter;

/// <summary>
/// This file is used to convert some CSharp classes to <see cref="MoonSharpUserDataAttribute"/> so they can be used in lua.
/// </summary>
namespace MoDuel.Tools.LuaExtensions {

    /// <summary>
    /// The C# Random class with access in lua.
    /// </summary>
    [MoonSharpUserData]
    public class SharpRandom : Random { }

}
