using MoDuel.Abilities;
using MoDuel.Data;
using System.Text.Json.Nodes;

namespace MoDuel.Heroes;

/// <summary>
/// A <see cref="Hero"/> that has been created or loaded from a file.
/// <para>Provides access to it's <see cref="Parameters"/>, its <see cref="TriggerReactions"/> and <see cref="ExplicitTriggerReactions"/>.</para>
/// </summary>
public class Hero(Package package, string id, JsonObject data) : LoadedAsset(package, id, data) {

    /// <summary>
    /// The initial abilities a hero will have.
    /// </summary>
    public readonly HashSet<Ability> InitialAbilities = [];

}
