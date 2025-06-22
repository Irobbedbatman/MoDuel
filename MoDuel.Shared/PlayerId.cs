namespace MoDuel.Shared;

/// <summary>
/// Identifier of a player. Should be unique when originally generated.
/// </summary>
public record PlayerId {

    /// <summary>
    /// The unique identifier of this player.
    /// </summary>
    public readonly long Id;

    public PlayerId(long id) {
        Id = id;
    }

}
