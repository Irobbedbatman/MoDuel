namespace MoDuel.Networking;

/// <summary>
/// Game provider that connects to a <see cref="DuelFlow"/> running in the same application.
/// </summary>
public class LocalGameProvider : GameProvider {

    /// <summary>
    /// The <see cref="DuelFlow"/> that commands are sent and received from.
    /// </summary>
    public readonly DuelFlow Flow;
    /// <summary>
    /// The player that is controlled by the player.
    /// </summary>
    public readonly Player LocalPlayer;

    public LocalGameProvider(DuelFlow flow, Player localPlayer) : base(localPlayer.UserId) {

        Flow = flow;
        LocalPlayer = localPlayer;

        // Retrieve the global commands from the duel flow.
        Flow.OutBoundDelegate += (s, e) => {
            ReceiveCommand?.Invoke(e.RequestId, CleanArray(e.Arguments));
        };

        // Retrieve the local commands sent to the player specifically.
        LocalPlayer.OutBoundDelegate += (s, e) => {
            ReceiveCommand?.Invoke(e.RequestId, CleanArray(e.Arguments));
        };
    }

    /// <inheritdoc/>
    public override void SendCommand(string commandId, params object[] args) {
        throw new NotImplementedException();
    }
}
