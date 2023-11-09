namespace MoDuel.Client;

/// <summary>
/// A block to send to a client that allows for reconstructing information based on the client settings.
/// </summary>
/// <param name="Key">A string key that the client will understand in the context it receives the <see cref="ParametrizedBlock"/>.</param>
/// <param name="Params">The data payload the client will receive.</param>
public record ParametrizedBlock(string Key, params object[] Params) { }
