using MoDuel.Shared;
using MoDuel.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Networking.Messages.Matchmaking;

/// <summary>
/// The information passed to the duel match service when creating a duel host.
/// </summary>
public class DuelHostingInfo {

    public bool IsPlayer1Ai = false;
    public bool IsPlayer2Ai = true;

    public string? Password = null;


    public PlayerMeta? AiPlayer1Data;

    public PlayerMeta? AiPlayer2Data;

    public DuelSettings Settings { init; get; } = new DuelSettings();

}
