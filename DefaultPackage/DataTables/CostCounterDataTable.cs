using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.Shared.Structures;

namespace DefaultPackage.DataTables;

/// <summary>
/// A data table to handle a one counter from a resource cost.
/// </summary>
public class CostCounterDataTable : DataTable {

    public Player? Player {
        get => Get<Player>("Player");
        set => this["Player"] = value;
    }

    public ResourceCounter? Cost {
        get => Get<ResourceCounter>("Cost");
        set => this["Cost"] = value;
    }
    public CostCounterDataTable() { }

    public CostCounterDataTable(Player player, ResourceCounter cost) {
        Player = player;
        Cost = cost;
    }

}
