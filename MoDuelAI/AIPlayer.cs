using MoDuel.Flow;
using MoDuel.Players;
using MoDuel.Tools;

namespace MoDuel.AI;

/// <summary>
/// Automated instance of <see cref="Player"/> on a locally provided <see cref="DuelFlow"/>.
/// </summary>
/// TODO: AI
public class AIPlayer(DuelFlow flow, Player player, ManagedRandom random) {

    /// <summary>
    /// The player the ai will automate actions for,
    /// </summary>
    public Player Player = player;
    /// <summary>
    /// The duel flow the ai will need to determine decisions.
    /// </summary>
    public DuelFlow Flow = flow;
    /// <summary>
    /// The managed random that can be used to keep the AI consistent.
    /// <para>If also used by duel flow the whole duel outcome will be fixed.</para>
    /// </summary>
    public ManagedRandom Random = random;

    /// <summary>
    /// The looping logic of the ai. TODO: Change to utilize signals.
    /// </summary>
    public void Loop() {

        // Stop the loop once the duel finishes.
        while (Flow.State.NotFinished) {

            // Delay such that the loop isn't constantly run.
            System.Threading.Thread.Sleep(500);

            // The AI doesn't perform actions on opponent turn.
            if (Flow.State.CurrentTurn.Owner != Player) {
                System.Threading.Thread.Sleep(500);
                continue;
            }

            // If the player has no cards force a revive.
            if (CheckRevive())
                continue;

            // The list of random commands the ai might take. Weighing charge as amore common action.
            List<Action> commands = [
                () => Flow.EnqueueCommand(Player, "CMDCharge"),
                () => Flow.EnqueueCommand(Player, "CMDCharge"),
                () => Flow.EnqueueCommand(Player, "CMDCharge")
            ];

            // Check to see if leveling up is a valid action.
            LevelUp(commands);

            // Check to see if cards in the hand could be played.
            PlayHandCards(commands);

            // If there are no commands the ai just charges. However currently this should never be the case because commands already starts with values.
            if (commands.Count == 0) {
                Flow.EnqueueCommand(Player, "CMDCharge");
                System.Threading.Thread.Sleep(1000);
                continue;
            }

            // Send a command to the server.
            var command = Random.NextItem(commands);
            command?.Invoke();

            // Sleep before sending another command.
            System.Threading.Thread.Sleep(1000);

        }

    }

    /// <summary>
    /// Checks to see what cards can be played in the hand and adds play actions to the <paramref name="commands"/> pool.
    /// </summary>
    private void PlayHandCards(List<Action> commands) {
        // Check to see if a card could be used or should be used.
        foreach (var card in Player.Hand) {

            // Assume the card can be played into any empty slot on the ai player's field.
            foreach (var slot in Player.Field.GetEmptySlots()) {
                commands.Add(() => {
                    Flow.EnqueueCommand(Player, "CMDPlayCard", card.Index, slot.Index);
                });
            }

            // Any card in the hand can also be discarded.
            commands.Add(() => {
                Flow.EnqueueCommand(Player, "CMDDiscard", card.Index);
            });
        }
    }

    /// <summary>
    /// Check to see if the player should be forced to revive and send a command to do so.
    /// </summary>
    /// <returns>True if the revive command was sent.</returns>
    private bool CheckRevive() {

        // Force a revive if have is empty and there are cards in grave.
        if (Player.Hand.Count == 0 && Player.Grave.Count > 0) {

            Flow.EnqueueCommand(Player, "CMDRevive");
            return true;
        }
        return false;
    }

    private void LevelUp(List<Action> commands) {
        throw new NotImplementedException();
    }

}
