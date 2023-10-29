using MoDuel.Data;
using MoDuel.State;

namespace MoDuel.Tools;

/// <summary>
/// Thread level values that are recorded for each thread.
/// </summary>
public static class ThreadContext {

    /// <summary>
    /// The <see cref="DuelState"/> used on the current thread.
    /// </summary>
    [ThreadStatic]
    private static DuelState? duelState;

    /// <summary>
    /// The <see cref="DuelState"/> used on the current thread.
    /// </summary>
    public static DuelState DuelState {
        get {
            if (duelState == null) {
                throw new ThreadStateException("The current thread does not have a duel state assigned. The ThreadContext class is only suitable to be used in a game loop thread.");
            }
            return duelState;
        }
        internal set => duelState = value;
    }

    /// <summary>
    /// The <see cref="PackageCatalogue"/> used on the current thread.
    /// </summary>
    public static PackageCatalogue? Catalouge => DuelState?.PackageCatalogue;

    /// <summary>
    /// Get the package with the provided <paramref name="packageName"/> that is safe to use on the current thread.
    /// </summary>
    public static Package? GetPackageInstance(string packageName) => Catalouge?.GetPackage(packageName);


}
