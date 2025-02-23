using MoDuel.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel.Networking.Messages.Matchmaking;


/// <summary>
/// Information sent by a player when they connect to a duel.
/// </summary>
public class DuelJoinInfo {

    public bool Spectating = false;

    public string PlayerId = string.Empty;

    public PlayerMeta? MetaData = null;


}
