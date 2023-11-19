using DefaultPackage.ContentLookups;
using MoDuel;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Json;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.State;
using MoDuel.Triggers;
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
        DuelState state = player.Context;
        state.Trigger("BeforeCharge", player);
        var resource = state.Random.NextItem(player.ResourcePool)?.Resource;

        OverwriteTable overwrite = new() {
            { "Player", player },
            { "Resource", resource },
            { "Amount", 1 },
            { "Action", new ActionFunction(GainResource) }
        };

        state.OverwriteTrigger("ChargeOverwrite", overwrite);
        if (overwrite["Player"] is not Player newPlayer) return;
        resource = overwrite["Resource"] as ResourceType;
        if (resource == null) return;
        int? amount = overwrite["Amount"] as int?;
        if (amount == null) return;
        GainResource(newPlayer, resource, amount.Value);
        state.Trigger("AfterCharge", newPlayer, resource, amount.Value);

    }

    /// <summary>
    /// Parses a json token to a <see cref="ResourceCost"/>.
    /// </summary>
    public static ResourceCost ParseTokenToCost(JsonNode token, Package package, int level) {

        var cost = new ResourceCost();

        if (token is not JsonObject)
            return cost;

        foreach (var property in token.GetProperties()) {
            cost.Add(ParseTokenToCounter(property, package, level));
        }

        return cost;
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
            GainResource(player, counter.Resource, 1);
        }
    }

    /// <summary>
    /// Grants a player the provided <paramref name="amount"/> of the provided <paramref name="resource"/>.
    /// </summary>
    [ActionName("GainResource")]
    public static void GainResource(Player player, ResourceType resource, int amount) {
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
        player.Context.Trigger("GainedResource", player, resource, amount);

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
        player.Context.Trigger("DrainedResource", player, resource, amount);

    }

    /// <summary>
    /// Gets the cap for the provided resource <paramref name="type"/>. Using it's explicit reaction if it has one.
    /// </summary>
    [ActionName(nameof(GetResourceCap))]
    public static int GetResourceCap(this ResourceType type) {
        var action = type.GetExplicitReaction("GetCap");
        if (!action.IsAssigned)
            return DEFAULT_RESOURCE_CAP;
        else
            return action.Call();
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
    public static void DefaultPayAmount(ResourceType type, Player player, int amount) {
        var counter = player.ResourcePool[type];
        if (counter != null) {
            // TODO CLIENT: client effects
            counter.Amount -= amount;
        }
    }

    /// <summary>
    /// Checks to see if a player can pay a <paramref name="cost"/>.
    /// </summary>
    /// <returns>True if the cost can be payed.</returns>
    [ActionName(nameof(CanPayCost))]
    public static bool CanPayCost(this Player player, ResourceCost cost) {
        ResourcePool pool = player.ResourcePool;

        // Check to see if each resource can be payed.
        foreach (var counter in cost) {
            var resource = counter.Resource;
            bool result = resource.FallbackTrigger("CanPay", new ActionFunction(DefaultCanPayAmount), player, counter.Amount);
            if (result == false)
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

        foreach (var counter in cost) {
            var resource = counter.Resource;
            resource.FallbackTrigger("Pay", new ActionFunction(DefaultPayAmount), player, counter.Amount);
            // TODO CLIENT: effects but should it just be through the above trigger.
        }

        return true;

    }


}
