using MoDuel.Serialization;
using MoDuel.State;

namespace MoDuel;

/// <summary>
/// Base class for various objects that each have unique identifiers that can be used to discern them.
/// </summary>
[SerializeReference]
public abstract class Target {

    /// <summary>
    /// The unique identifier of this target.
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// The registry this target is contained within.
    /// </summary>
    protected readonly TargetRegistry Registry;

    /// <summary>
    /// Instance values held on this <see cref="Target"/>.
    /// <para>Serialized with state of the duel.</para>
    /// </summary>
    public readonly Dictionary<string, object?> Values = [];

    public Target(TargetRegistry registry, int? index = null) {
        Registry = registry;
        Index = index ?? registry.GetNextIndex();
        registry.Assign(this);
    }

    /// <summary>
    /// Finalizer of a Target. Frees up the unique index to use elsewhere.
    /// </summary>
    ~Target() {
        try {
            Registry.FreeTarget(this);
        }
        catch { }
    }

    /// <summary>
    /// Sets a value in <see cref="Values"/>.
    /// </summary>
    public void SetValue(string key, object? val) => Values[key] = val;

    /// <summary>
    /// Retrieve a value from <see cref="Values"/>.
    /// <para>Returns null if the <paramref name="key"/> is not found.</para>
    /// </summary>
    public dynamic? GetValue(string key) {
        if (Values.TryGetValue(key, out var val))
            return val;
        return null;
    }

    /// <summary>
    /// Indexed accessor for <see cref="Values"/>.
    /// </summary>
    public dynamic? this[string key] {
        get => GetValue(key);
        set {
            SetValue(key, value);
        }
    }

    /// <summary>
    /// Check to see if <see cref="Values"/> has a specified key.
    /// </summary>
    public bool HasValue(string key) => Values.ContainsKey(key);

    /// <summary>
    /// Retrieves a value from <see cref="Values"/>
    /// <para>Returns the value provided in fallback if no value was found and also sets the corresponding value in <see cref="Values"/>.</para>
    /// </summary>
    public dynamic? GetOrSetValue(string key, object? fallback) {
        if (Values.TryGetValue(key, out var val))
            return val;
        SetValue(key, fallback);
        return fallback;
    }

    /// <summary>
    /// Removes a value from <see cref="Values"/>.
    /// </summary>
    public void ClearValue(string key) => Values.Remove(key);

}
