using LiteNetLib.Utils;

namespace MoDuel.Networking;

/// <summary>
/// The abstract logic to communicate with a <see cref="MoDuel.DuelFlow"/> via commands.
/// </summary>
public abstract class GameProvider {

    /// <summary>
    /// The id of the player on this machine.
    /// </summary>
    public readonly string LocalID = "";

    protected GameProvider(string id) {
        LocalID = id;
    }

    /// <summary>
    /// The method used to send commands to a <see cref="MoDuel"/> duel flow.
    /// </summary>
    public abstract void SendCommand(string commandId, params object[] args);

    /// <summary>
    /// The action that is invoked when a message is received from a <see cref="MoDuel"/> duel flow.
    /// </summary>
    public Action<string, object[]>? ReceiveCommand;

}
