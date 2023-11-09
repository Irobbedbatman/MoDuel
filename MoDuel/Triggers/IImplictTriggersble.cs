namespace MoDuel.Triggers;

/// <summary>
/// Objects that respond to global triggers; granting response.
/// <para>Doesn't call the response; simply returns it such that it can be called when necessary.</para>
/// </summary>
public interface IImplicitTriggerable {

    /// <summary>
    /// Gets the response this object will have when the <paramref name="trigger"/> is called.
    /// </summary>
    /// <param name="trigger">The keyword that will return the reaction from the <see cref="IImplicitTriggerable"/>.</param>
    /// <returns>The reaction to the <paramref name="trigger"/> or a <see cref="ActionFunction"/> with no assigned effect if an reaction couldn't be found.</returns>
    ActionFunction GetImplicitReaction(string trigger) {
        return new();
    }

    /// <summary>
    /// Check to see if this <see cref="IImplicitTriggerable"/> has a reaction to the <paramref name="trigger"/>.
    /// </summary>
    /// <returns>True if this <see cref="IImplicitTriggerable"/> reacts to the <paramref name="trigger"/>.</returns>
    bool HasImplicitReaction(string trigger) {
        return false;
    }

}
