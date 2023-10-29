namespace MoDuel.Client;

/// <summary>
/// A block to send to a client that allows for reconstucing information based on the client seetings.
/// </summary>
/// <param name="Key">A string key that the client will understand in the context it recieves the <see cref="ParametizedBlock"/>.</param>
/// <param name="Params">The data payload the client will recieve.</param>
public record ParametizedBlock(string Key, params object[] Params) { }
