using MoDuel.Shared.Structures;

namespace MoDuel.Triggers;

/// <summary>
/// THe class that contains the execution of pipelines.
/// </summary>
public static class TriggerPipeline {

    /// <summary>
    /// Execute a series of a reactions in sequence.
    /// </summary>
    /// <param name="trigger">The trigger that created the reactions.</param>
    /// <param name="reactions">The reactions to invoke.</param>
    /// <param name="data">The data to pass to each reaction.</param>
    public static void Execute(Trigger trigger, IEnumerable<TriggerReaction> reactions, DataTable data) {

        foreach (var reaction in reactions) {

            // Create a clone of the data, this will be sent only to this reaction.
            var clone = data.Clone<DataTable>();

            clone["_Valid"] = true; // If the reaction should be executed.
            clone["_OriginalData"] = data; // The original data before any changes.
            clone["_Reaction"] = reaction;
            clone["_Action"] = reaction.Action; // The action that will be executed after overrides.

            // Create a sub-trigger for the override.
            var overrideTrigger = trigger.CreateSubOverrideTrigger(reaction);
            var overrideReactions = overrideTrigger.GetReactions();

            ExecuteOverride(overrideTrigger, overrideReactions, ref clone);

            // Check and use the meta values to execute the newest changes.
            if (clone.Get<bool>("_Valid")) {
                clone.Get<ActionFunction>("_Action")?.Call(reaction.Source.Ability, trigger, data);
            }
        }
    }


    /// <summary>
    /// Execute a pipeline from the provided trigger to overwrite a data table.
    /// </summary>
    /// <typeparam name="T">The type of data that will be sent through the pipeline.</typeparam>
    /// <param name="trigger">The trigger used to start the pipeline.</param>
    /// <param name="reactions">The reactions to pass through the pipeline.</param>
    /// <param name="data">The data that will be sent through and manipulated in the pipeline.</param>
    public static void ExecuteOverride<T>(Trigger trigger, IEnumerable<TriggerReaction> reactions, ref T data) where T : DataTable {

        // Provide the original data in the table as a meta value.
        data["_OriginalData"] = data;

        foreach (var reaction in reactions) {

            // Create a clone of the data before any changes.
            var clonedData = data.Clone<T>();

            // Execute the reaction temporarily.
            reaction.Action.Call(reaction.Source.Ability, trigger, clonedData);

            // No change in data, no need to validate.
            if (clonedData.SequenceEqual(data)) {
                continue;
            }

            // Execute the validation.
            if (ExecuteValidation(trigger, reaction, data, clonedData)) {
                // If valid update the actual data to the cloned data.
                data = clonedData;
            }
        }
    }

    /// <summary>
    /// Execute validation on a reaction, ensuring it should be executed.
    /// </summary>
    /// <param name="trigger">The validation trigger.</param>
    /// <param name="reaction">The reaction to validate. </param>
    /// <param name="before">The data before the reaction.</param>
    /// <param name="after">THe data after the reaction.</param>
    /// <returns>The result of the validation.</returns>
    public static bool ExecuteValidation(Trigger trigger, TriggerReaction reaction, DataTable before, DataTable after) {

        // Default to a validated state.
        var data = new ResultDataTable<bool>(true) {
            ["Trigger"] = trigger,
            ["Reaction"] = reaction,
            ["DataBefore"] = before,
            ["DataAfter"] = after
        };

        // Create the trigger to get validators.
        var validationTrigger = trigger.CreateSubValidationTrigger(reaction);

        // Get the reactions to the trigger.
        var reactions = trigger.State.GetReactions(validationTrigger);

        var tempCast = (DataTable)data;

        // Run pipeline on the validation state.
        ExecuteOverride(validationTrigger, reactions, ref tempCast);

        data = (ResultDataTable<bool>)tempCast;
        return data.GetResult();
    }

}
