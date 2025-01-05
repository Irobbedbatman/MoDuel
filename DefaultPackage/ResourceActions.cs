using DefaultPackage.ContentLookups;
using DefaultPackage.DataTables;
using MoDuel;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.Shared.Json;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;
using MoDuel.Triggers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace DefaultPackage;

/// <summary>
/// Actions that relate to the resource system.
/// </summary>
public static class ResourceActions {

    /// <summary>
    /// The default maximum of a resource a player can have.
    /// </summary>
    public const int DEFAULT_RESOURCE_CAP = 20;

    /// <summary>
    /// The meditate command logic.
    /// </summary>
    [ActionName(nameof(Meditate))]
    public static void Meditate(Player player) {
        var source = new SourceCommand(nameof(Meditate), player);

        DuelState state = player.Context;

        DataTable data = new() {
            { "Player", player }
        };

        state.Trigger(new Trigger("BeforeMeditate", source, state, TriggerType.Implicit), data);
        var resource = state.Random.NextItem(player.ResourcePool)?.Resource;

        data["Resource"] = resource;
        data["Amount"] = 1;
        data["Action"] = new ActionFunction(GainResource);

        state.DataTrigger(new Trigger("Mediate", source, state, TriggerType.DataOverride), ref data);

        if (data["Player"] is not Player newPlayer) return;
        resource = data["Resource"] as ResourceType;
        if (resource == null) return;
        int? amount = data["Amount"] as int?;
        if (amount == null) return;
        if (data["Action"] is not ActionFunction action) {
            GlobalActions.Fizzle();
            return;
        }

        action.Call(data);

        state.Trigger(new Trigger("AfterMeditate", source, state, TriggerType.Implicit), data);

    }

    /// <summary>
    /// Parses a json token to a <see cref="ResourceCounter"/>.
    /// </summary>
    public static ResourceCounter ParseTokenToCounter(KeyValuePair<string, JsonNode> property, Package package, int level) {
        ResourceType? type = package.Catalogue.LoadResourceType(property.Key, package);
        int? amount = property.Value?.BaseFallbackGetOrHighest(level).ToRawValueOrFallback(0);
        if (type == null || amount == null || amount.Value < 1)
            return new ResourceCounter(ResourceTypes.Void);
        return new ResourceCounter(type, amount.Value);
    }

    /// <summary>
    /// Parses a json token to a <see cref="ResourcePool"/>.
    /// </summary>
    public static ResourcePool ParseTokenToPool(JsonNode token, Package package, int level) {

        var counters = new List<ResourceCounter>();

        if (token is not JsonObject)
            return new ResourcePool(counters);

        foreach (var property in token.GetProperties()) {
            counters.Add(ParseTokenToCounter(property, package, level));
        }

        return new ResourcePool(counters);

    }

    /// <summary>
    /// Grants the player one of each resource.
    /// </summary>
    [ActionName("GetEachResource")]
    public static void GainEachResource(Player player) {
        foreach (var counter in player.ResourcePool) {

            var data = new DataTable() {
                ["Player"] = player,
                ["Resource"] = counter.Resource,
                ["Amount"] = 1
            };

            GainResource(data);
        }
    }

    /// <summary>
    /// Grants a player the provided <paramref name="amount"/> of the provided <paramref name="resource"/>.
    /// </summary>
    [ActionName("GainResource")]
    public static void GainResource(DataTable data) {

        var player = data.Get<Player>("Player");
        var resource = data.Get<ResourceType>("Resource");
        var amount = data.Get<int>("Amount");

        if (player == null || resource == null || resource == ResourceType.Missing) {
            return;
        }

        if (amount <= 0) {
            GlobalActions.Fizzle();
            return;
        }
        var counter = player.ResourcePool[resource];
        if (counter == null) {
            GlobalActions.Fizzle();
            return;
        }

        counter.Amount = Math.Clamp(counter.Amount + amount, 0, resource.GetResourceCap());
        player.Context.SendRequest(new ClientRequest("GainResource", player.Index, resource.Id, amount));

        var trigger = new Trigger("GainedResource", new Source(), player.Context, TriggerType.Implicit);

        player.Context.Trigger(trigger, data);

    }


    /// <summary>
    /// Reduces the <paramref name="amount"/> of a certain <paramref name="resource"/> the player has.
    /// </summary>
    [ActionName("DrainResource")]
    public static void DrainResource(Player player, ResourceType resource, int amount) {
        if (amount <= 0) {
            GlobalActions.Fizzle();
            return;
        }
        var counter = player.ResourcePool[resource];
        if (counter == null) {
            GlobalActions.Fizzle();
            return;
        }

        counter.Amount = Math.Clamp(counter.Amount - amount, 0, resource.GetResourceCap());
        player.Context.SendRequest(new ClientRequest("DrainResource", player.Index, resource.Id, amount));

        var data = new DataTable() {
            ["Player"] = player,
            ["Resource"] = resource,
            ["Amount"] = amount
        };

        var trigger = new Trigger("DrainedResource", new Source(), player.Context, TriggerType.Implicit);

        player.Context.Trigger(trigger, data);

    }

    /// <summary>
    /// Gets the cap for the provided resource <paramref name="type"/>. Using it's explicit reaction if it has one.
    /// </summary>
    [ActionName(nameof(GetResourceCap))]
    public static int GetResourceCap(this ResourceType type) {

        var data = new DataTable() {
            ["Type"] = type,
            ["Cap"] = 20
        };

        type.AbilityDataTrigger("GetCap", ref data);
        return data.Get<int>("Cap");
    }

    /// <summary>
    /// The default check to see if a player can pay the provided <paramref name="amount"/> of the provided resource <paramref name="type"/>..
    /// </summary>
    [ActionName(nameof(DefaultCanPayAmount))]
    public static bool DefaultCanPayAmount(ResourceType type, Player player, int amount) {
        var counter = player.ResourcePool[type];
        return counter != null && counter.Amount >= amount;
    }

    /// <summary>
    /// Makes the player pay the <paramref name="amount"/> of the provided resource <paramref name="type"/>.
    /// </summary>
    [ActionName(nameof(DefaultPayAmount))]
    public static void DefaultPayAmount(Player player, ResourceCounter cost) {
        var type = cost.Resource;
        var counter = player.ResourcePool[type];
        if (counter != null) {
            // TODO CLIENT: client effects
            counter.Amount -= cost.Amount;
        }
    }

    /// <summary>
    /// Convert the resource cost to a set of counters that can be directly used.
    /// </summary>
    public static IEnumerable<ResourceCounter> ConvertToFixedCost(this ResourceCost cost) {
        ResourceCost fixedCost = [];
        // TODO: implement custom logic.
        return cost.GetAsCounters();
    }

    /// <summary>
    /// Checks to see if a player can pay a <paramref name="cost"/>.
    /// </summary>
    /// <returns>True if the cost can be payed.</returns>
    [ActionName(nameof(CanPayCost))]
    public static bool CanPayCost(this Player player, ResourceCost cost) {
        var fixedCost = cost.ConvertToFixedCost();

        // Check to see if each resource can be payed.
        foreach (var counter in fixedCost) {
            var resource = counter.Resource;

            var data = new CostCounterDataTable(player, counter) {
                ["Result"] = DefaultCanPayAmount(resource, player, counter.Amount),
            };

            resource.AbilityDataTrigger("CanPay", ref data);

            var result = data.Get<bool>("Result");

            if (!result)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Makes the player pay the provided <paramref name="cost"/>.
    /// <para>First checks the cost can be payed.</para>
    /// </summary>
    /// <returns>True if the cost was payed.</returns>
    [ActionName(nameof(PayCost))]
    public static bool PayCost(this Player player, ResourceCost cost) {

        ResourcePool pool = player.ResourcePool;

        if (!CanPayCost(player, cost)) {
            return false;
        }

        var fixedCost = cost.ConvertToFixedCost();

        foreach (var counter in fixedCost) {

            var resource = counter.Resource;

            var data = new CostCounterDataTable(player, counter) {
                ["PayAction"] = new ActionFunction(DefaultPayAmount)
            };

            resource.AbilityDataTrigger("GetPayAction", ref data);

            var action = data.Get<ActionFunction>("PayAction");

            action?.Call(player, counter);

            // TODO CLIENT: effects but should it just be through the above trigger.
        }

        return true;

    }


}
