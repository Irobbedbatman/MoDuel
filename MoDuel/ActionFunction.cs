using MoDuel.Serialization;
using MoDuel.Shared;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MoDuel;

/// <summary>
/// Wrapper around a <see cref="Delegate"/> that allow it to be reloaded when serializing.
/// </summary>
[SerializeReference]
public class ActionFunction : IReloadable {

    /// <summary>
    /// Whether the action function has been loaded. A blank action function is not considered loaded.
    /// </summary>
    public bool Loaded { get; private set; } = false;

    /// <summary>
    /// Constructor for an unassigned <see cref="ActionFunction"/>.
    /// </summary>
    public ActionFunction() { }

    /// <summary>
    /// Constructor for an assigned <see cref="ActionFunction"/>.
    /// </summary>
    /// <param name="fullItemPath">The path the <see cref="ActionFunction"/> was loaded from.</param>
    /// <param name="_delegate">The <see cref="System.Delegate"/> that will be called internally.</param>
    public ActionFunction(string fullItemPath, Delegate _delegate) {
        FullItemPath = fullItemPath;
        Delegate = _delegate;
        Loaded = true;
    }

    public ActionFunction(Delegate _delegate) {
        Delegate = _delegate;
        Loaded = true;
    }

    /// <summary>
    /// Late assignment to allow for temporary memory allocation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="ActionFunction"/> has already been assigned.</exception>
    public void Assign(string fullItemPath, Delegate _delegate) {
        if (IsAssigned) {
            throw new InvalidOperationException($"An ActionFunction was assigned a new delegate when it had already been assigned one. Old Name: {FullItemPath}, New Name: {fullItemPath}.");
        }
        FullItemPath = fullItemPath;
        Delegate = _delegate;
        Loaded = true;
    }

    /// <summary>
    /// The internal delegate that the <see cref="ActionFunction"/> is wrapped around.
    /// </summary>
    private Delegate? Delegate;

    /// <summary>
    /// The path that <see cref="ActionFunction"/> was loaded from.
    /// </summary>
    public string? FullItemPath { get; private set; } = null;

    /// <summary>
    /// If <see cref="ActionFunction"/> has something to execute.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Delegate))]
    public bool IsAssigned => Delegate != null;

    /// <summary>
    /// Call the <see cref="ActionFunction"/>.
    /// </summary>
    /// <param name="args">The arguments to pass into the <see cref="ActionFunction"/>.</param>
    /// <returns>The result of the <see cref="ActionFunction"/>.</returns>
    public dynamic? Call(params object?[]? args) {

        try {
            return Delegate?.DynamicInvoke(args);
        }
        catch (Exception ex) {
            Logger.Log(LogTypes.ActionFailed, $"ActionFunction.Call() failed.\n" +
                $"Exception: {ex.GetType()}"+
                $"\nCalled Method: {Delegate?.Method}" +
                $"\nMethod Type: {Delegate?.Method.DeclaringType}" +
                $"\nArgs: { string.Join(", ", args?.Select(a => a?.GetType().ToString() ?? "-") ?? [])}" +
                $"\nPath: {FullItemPath ?? "-"}" +
                $"\nStack: {new StackTrace(true)}");
            Debugger.Break();
            return default;
        }
    }

    /// <inheritdoc/>
    string? IReloadable.GetItemPath() {
        return FullItemPath;
    }
}
