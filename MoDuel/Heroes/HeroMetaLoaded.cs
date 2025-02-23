using MoDuel.Data;
using MoDuel.Shared.Data;
using System.Collections.Frozen;

namespace MoDuel.Heroes;

/// <summary>
/// The loaded version of a <see cref="HeroMeta"/>.
/// </summary>
public class HeroMetaLoaded {

    /// <summary>
    /// The player that owns the card. If null the card instance has no original owner.
    /// </summary>
    public readonly string? OwnerName;
    /// <summary>
    /// The loaded hero data that defines what the meta used in.
    /// </summary>
    public readonly Hero Hero;
    /// <summary>
    /// The raw meta data values.
    /// </summary>
    public readonly FrozenDictionary<string, object?> Values;

    /// <summary>
    /// Create a loaded hero meta from it's raw equivalent.
    /// </summary>
    /// <param name="raw">The raw data to read from.</param>
    /// <param name="data">The <see cref="PackageCatalogue"/> to load the hero from.</param>
    /// <returns>The created <see cref="HeroMetaLoaded"/> or null if there was an issue during loading resources.</returns>
    public static HeroMetaLoaded? CreateFromRawData(HeroMeta raw, PackageCatalogue data) {

        // Build the meta values from the data.
        var values = new Dictionary<string, object?>();
        foreach (var value in raw.Values) {
            values.Add(value.Key, value.Value);
        }

        // Attempt to load the hero, return null if the card does not exist.
        var hero = data.LoadHero(raw.HeroId);
        if (hero == null) return null;

        return new HeroMetaLoaded(hero, values.ToFrozenDictionary());
    }

    /// <summary>
    /// Creates a hero meta from this <see cref="HeroMetaLoaded"/>.
    /// </summary>
    /// <param name="hero">A new hero type to use. If no value is provided instead the same <see cref="Hero"/> will be used.</param>
    public HeroMetaLoaded Derive(Hero? hero = null) => new(hero ?? Hero, Values);

    /// <summary>
    /// Creates a new hero meta from a <see cref="Hero"/>
    /// </summary>
    /// <param name="hero">The hero that will be used.</param>
    /// <param name="values">A set of values to be used for <see cref="Values"/>.</param>
    /// <returns>The created <see cref="HeroMetaLoaded"/>.</returns>
    public static HeroMetaLoaded CreateNew(Hero hero, IDictionary<string, object?>? values = null) => new(hero, (values ?? new Dictionary<string, object?>()).ToFrozenDictionary());

    private HeroMetaLoaded(Hero hero, FrozenDictionary<string, object?> values) {
        Hero = hero;
        Values = values;

    }

}
