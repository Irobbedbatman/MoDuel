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
    /// The method used to send commands to a <see cref="MoDuel.DuelFlow"/>
    /// </summary>
    public abstract void SendCommand(string commandId, params object[] args);

    /// <summary>
    /// The action that is invoked when a message is received from a <see cref="MoDuel.DuelFlow"/>.
    /// </summary>
    public Action<string, object[]>? ReceiveCommand;

    /// <summary>
    /// Cleans <see cref="DynValue"/>s of their ownership so the values can be sent from backend to frontend and vise-versa.
    /// </summary>
    protected static object[] CleanArray(object[] values) {
        var writer = new NetDataWriter();
        MoDuel.Networking.DynValueSerialization.SerializeArray(values, writer);
        return MoDuel.Networking.DynValueSerialization.DeserializeArray(new NetDataReader(writer)) ?? Array.Empty<DynValue>(); ;
    }

}
