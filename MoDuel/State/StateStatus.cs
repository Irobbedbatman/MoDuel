using MoDuel.Players;

namespace MoDuel.State;

// Look at DuelState.cs for documentation.
public partial class DuelState {

    /// <summary>
    /// True if the duel has started.
    /// <para>Doesn't affect <see cref="Finished"/> nor is it affected by it.</para>
    /// </summary>
    public bool Started { get; set; } = false;

    /// <summary>
    /// True if the duel has finished.
    /// <para>Doesn't affect <see cref="Started"/>.</para>
    /// </summary>
    public bool Finished { get; set; } = false;

    /// <summary>
    /// True only if <see cref="Started"/> and not <see cref="Finished"/>.
    /// </summary>
    public bool Ongoing => Started && !Finished;

    /// <summary>
    /// True if the game hasn't started yet.
    /// </summary>
    public bool NotStarted {
        get => !Started;
        set => Started = !value;
    }

    /// <summary>
    /// True if the game hasn't finished yet.
    /// <para>True even if <see cref="Started"/> is false.</para>
    /// </summary>
    public bool NotFinished {
        get => !Finished;
        set => Finished = !value;
    }

    /// <summary>
    /// The data of any given turn, new turns can be created using <see cref="NewTurn(Player, uint)"/>.
    /// </summary>
    public TurnData CurrentTurn { get; private set; }

    /// <summary>
    /// The amount of turns that have passed this duel.
    /// </summary>
    public int TurnCount { get; private set; } = 0;

    /// <summary>
    /// The <see cref="TurnData"/> history of the duel.
    /// </summary>
    public readonly Dictionary<int, TurnData> TurnHistory = new();

    /// <summary>
    /// Creates a new turn data and updates the <see cref="CurrentTurn"/> and <see cref="TurnCount"/>.
    /// </summary>
    /// <param name="player">The player in control of the turn.</param>
    /// <param name="actionPoints">How many actions the <paramref name="player"/> will have to use.</param>
    /// <returns></returns>
    public TurnData NewTurn(Player player) {
        CurrentTurn.Owner.LastTurn = CurrentTurn;
        TurnCount++;
        var lastTurn = CurrentTurn;
        CurrentTurn = new TurnData(player, lastTurn);
        TurnHistory.Add(TurnCount, CurrentTurn);
        return CurrentTurn;
    }


}
