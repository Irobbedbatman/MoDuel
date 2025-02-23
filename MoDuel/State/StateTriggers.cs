using MoDuel.Abilities;
using MoDuel.Shared;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.Triggers;

namespace MoDuel.State;

// Look at DuelState.cs for documentation.
public partial class DuelState {

    /// <summary>
    /// How many triggers have activated within another trigger.
    /// </summary>
    private float currentTriggerDepth = 1;

    /// <summary>
    /// Get all the reactors and their reaction to the provided <paramref name="trigger"/>.
    /// <para>Ordered from what should be used as the most impactfully to the least impactfully and may need to be reversed.</para>
    /// </summary>
    public List<TriggerReaction> GetReactions(Trigger trigger) {

        // Ensure the duel is still happening.
        if (!Ongoing)
            return [];

        var turnOwner = CurrentTurn.Owner;
        var opposing = turnOwner.GetOpposingPlayer();

        // Ensure both players are valid.
        if (turnOwner == null || opposing == null)
            return [];

        var reactingEntities = new List<IAbilityEntity> {
            GlobalEntity,
        };

        reactingEntities.AddRange(PackageCatalogue.GetAllLoadedResourceTypes());
        reactingEntities.Add(turnOwner.Hero);
        reactingEntities.Add(opposing.Hero);
        reactingEntities.AddRange(CardManager.CardInstances);

        var reactions = reactingEntities.SelectMany(e => e.GetReactions(trigger)).ToList();

        reactions.Sort(new ReactionComparer(trigger));

        return reactions;
    }

    /// <summary>
    /// Call an implicit trigger which will call the reactions in sequence.
    /// <para>Reactions are executed from most impactful to least impactful.</para>
    /// </summary>
    /// <param name="trigger">The trigger that was raised..</param>
    /// <param name="data">The data to pass to the reactions.</param>
    public void Trigger(Trigger trigger, DataTable data) {

        // Block execution at the set depth.
        if (currentTriggerDepth >= Settings.MaxTriggerDepth) {
            Logger.Log(LogTypes.TriggerFailed, "Max trigger depth reached.");
            return;
        }
        currentTriggerDepth++;

        Logger.Log(LogTypes.TriggerCalled, "Trigger called: " + trigger.Key);

        TriggerPipeline.Execute(trigger, GetReactions(trigger), data);

        currentTriggerDepth--;
    }

    /// <summary>
    /// Call an implicit trigger for which the reactions will modify the referenced data,
    /// <para>Reactions are executed from least impactful to most impactful.</para>
    /// </summary>
    /// <param name="trigger">The trigger that was raised.</param>
    /// <param name="data">The data that will be modified by the reactions.</param>
    public void DataTrigger<T>(Trigger trigger, ref T data) where T : DataTable {

        // Block execution at the set depth.
        if (currentTriggerDepth >= Settings.MaxTriggerDepth) {
            Logger.Log(LogTypes.TriggerFailed, "Max trigger depth reached.");
            return;
        }
        currentTriggerDepth++;

        Logger.Log(LogTypes.TriggerCalled, "DataTrigger called: " + trigger.Key);

        TriggerPipeline.ExecuteOverride(trigger, GetReactions(trigger).Reverse<TriggerReaction>(), ref data);

        currentTriggerDepth--;
    }

    /// <summary>
    /// Call an explicit trigger which will call the reactions from the entity in sequence.
    /// <para>Reactions are executed from most impactful to least impactful.</para>
    /// </summary>
    /// <param name="trigger">The trigger that was raised.</param>
    /// <param name="data">The data to pass to the reactions.</param>
    public void ExplicitTrigger(Trigger trigger, DataTable data) {

        // Block execution at the set depth.
        if (currentTriggerDepth >= Settings.MaxTriggerDepth) {
            Logger.Log(LogTypes.TriggerFailed, "Max trigger depth reached.");
            return;
        }

        // Explicit triggers require an entity.
        if (trigger.Source is not SourceEntity sender)
            return;

        // Get all the abilities from the explicit trigger entity.
        var abilities = sender.Entity.GetAbilities();

        // Get all the reactions to the trigger, with most impactful reactions being called first.
        var references = abilities.Order().Select(a => a.GetReaction(trigger)).OfType<TriggerReaction>();

        currentTriggerDepth++;

        Logger.Log(LogTypes.TriggerCalled, "ExplicitTrigger called: " + trigger.Key);

        TriggerPipeline.Execute(trigger, references, data);

        currentTriggerDepth--;
    }

    /// <summary>
    /// Call an explicit trigger for which the entity will react and modify the referenced data.
    /// <para>After the entity has reacted, implicit listener to a <see cref="TriggerType.ExplicitDataOverride"/> will be able to react.</para>
    /// </summary>
    /// <param name="trigger">The trigger that was raised.</param>
    /// <param name="data">The data that will be modified by any reactions.</param>
    public void ExplicitDataTrigger<T>(Trigger trigger, ref T data) where T : DataTable {

        // Block execution at the set depth.
        if (currentTriggerDepth >= Settings.MaxTriggerDepth) {
            Logger.Log(LogTypes.TriggerFailed, "Max trigger depth reached.");
            return;
        }

        // Explicit triggers require an entity.
        if (trigger.Source is not SourceEntity sender)
            return;

        currentTriggerDepth++;

        Logger.Log(LogTypes.TriggerCalled, "ExplicitDataTrigger called: " + trigger.Key);

        // Get all the abilities from the explicit trigger entity.
        var abilities = sender.Entity.GetAbilities();
        // Get all the reactions to the trigger.
        var references = abilities.OrderDescending().Select(a => a.GetReaction(trigger)).OfType<TriggerReaction>();

        TriggerPipeline.ExecuteOverride(trigger, references, ref data);

        // Allow any implicit trigger to override the explicit values.

        var globalOverrides = trigger.CreateSubExplicitOverrideTrigger();
        var responses = globalOverrides.GetReactions().Reverse();

        TriggerPipeline.ExecuteOverride(globalOverrides, responses, ref data);

        currentTriggerDepth--;

    }

}
