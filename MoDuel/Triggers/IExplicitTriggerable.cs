namespace MoDuel.Triggers;

/// <summary>
/// Objetcts that can explicitly be told to react to a certain trigger keyword.
/// </summary>
public interface IExplicitTriggerable {

    /// <summary>
    /// Get the response the object will have when it is infromed to react to the provided <paramref name="trigger"/>.
    /// </summary>
    /// <param name="trigger">The keyword that will return the reaction from the <see cref="IExplicitTriggerable"/>.</param>
    /// <returns>The reaction to the <paramref name="trigger"/> or a <see cref="ActionFunction"/> with no assigned effect if an reaction couldn't be found.</returns>
    ActionFunction GetExplicitReaction(string trigger);

    /// <summary>
    /// Trigger the Explicit trigger.
    /// </summary>
    /// <param name="trigger">The keyword used to get the reaction that will be called.</param>
    /// <param name="arguments">The arguments that will be passed to the reaction alongside this.</param>
    /// <returns>The result of the explicit trigger. null if their is no trigger.</returns>
    dynamic? Trigger(string trigger, params object?[] arguments);

    /// <summary>
    /// Triggers the explicit trigger. Calls the provided <paramref name="fallback"/> if no explicit trigger was found.
    /// </summary>
    /// <param name="trigger">The keyword used to get the reaction that will attempt to be called.</param>
    /// <param name="fallback">The action to call if no explicit trigger for the provided <paramref name="trigger"/> coudld be found.</param>
    /// <param name="arguments">The arguments that will be passed to the reaction alongside this.</param>
    /// <returns>The reasilt of the reaction to the <paramref name="trigger"/> or the result of <paramref name="fallback"/> if no reaction exists. </returns>
    dynamic? FallbackTrigger(string trigger, ActionFunction fallback, params object?[] arguments);

    /// <summary>
    /// Triggers the explicit trigger. Calls the provided <paramref name="fallback"/> if no explicit trigger was found.
    /// <para>Wraps <paramref name="fallback"/> into a runtime <see cref="ActionFunction"/>.</para>
    /// </summary>
    /// <param name="trigger">The keyword used to get the reaction that will attempt to be called.</param>
    /// <param name="fallback">The action to call if no explicit trigger for the provided <paramref name="trigger"/> coudld be found.</param>
    /// <param name="arguments">The arguments that will be passed to the reaction alongside this.</param>
    /// <returns>The reasilt of the reaction to the <paramref name="trigger"/> or the result of <paramref name="fallback"/> if no reaction exists. </returns>
    dynamic? FallbackTrigger(string trigger, Delegate fallback, params object?[] arguments);

}
