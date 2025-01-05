using MoDuel.Data;
using MoDuel.Shared.Json;
using System.Text.Json.Nodes;

namespace MoDuel.Resources;

/// <summary>
/// The individual cost of 
/// </summary>
public class CostComponent {

    /// <summary>
    /// The package that created the cost.
    /// </summary>
    public readonly Package? Package;

    /// <summary>
    /// The type associated with this resource component.
    /// </summary>
    public readonly ResourceType Type;

    /// <summary>
    /// The amount of the type that the resource will cost.
    /// </summary>
    public readonly int Amount;

    /// <summary>
    /// The raw data used to create the cost.
    /// </summary>
    public readonly JsonNode? Data;

    public CostComponent(ResourceType type, int amount) {
        Type = type;
        Amount = amount;

    }

    public CostComponent(Package sender, JsonNode data) {

        Package = sender;
        Data = data;

        switch (data.GetValueKind()) {
            case System.Text.Json.JsonValueKind.String:
                Type = sender.Catalogue.LoadResourceType(data.ToRawValue<string>() ?? "", sender) ?? ResourceType.Missing;
                Amount = 1;
                break;
            case System.Text.Json.JsonValueKind.Object:
                var typeString = data.Get("Type").ToRawValue<string>();
                var amount = data.Get("Amount").ToRawValue<int>();
                Type = sender.Catalogue.LoadResourceType(typeString ?? "", sender) ?? ResourceType.Missing;
                Amount = amount;
                break;
            case System.Text.Json.JsonValueKind.Array:
            default:
                Type = ResourceType.Missing;
                Amount = 0;
                break;
        }

    }

    /// <summary>
    /// Convert the cost to a generic counter, this does not reflect how it may actual be applied.
    /// </summary>
    public ResourceCounter ConvertToCounter() => new(Type, Amount);

}
