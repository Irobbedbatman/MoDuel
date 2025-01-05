using MoDuel.Cards;
using MoDuel.Client;
using MoDuel.Data;
using MoDuel.Data.Assembled;
using MoDuel.Players;
using MoDuel.Shared.Structures;
using MoDuel.Sources;
using MoDuel.State;
using MoDuel.Triggers;

namespace DefaultPackage;

/// <summary>
/// Actions for use by the duel system itself.
/// </summary>
public static class SystemActions {

    /// <summary>
    /// The system action called when the duel is over.
    /// <para>Informs the players who the winner is.</para>
    /// </summary>
    [ActionName(nameof(SysGameEnd))]
    public static void SysGameEnd(DuelState state) {

        var player1 = state.Player1;
        var player2 = state.Player2;

        // Check who the winner of the duel is.
        Player? winner = null;
        if (player1.IsAlive) winner = player1;
        if (player2.IsAlive) winner = player2;

        if (winner == null) {
            state.SendRequest(new ClientRequest("GameOverDraw"));
        }
        else {
            state.SendRequest(new ClientRequest("GameOver", winner));
        }
    }

    /// <summary>
    /// The system action called when the duel is started.
    /// <para>Setups the duel and starts the first turn.</para>
    /// </summary>
    [ActionName(nameof(SysGameStart))]
    public static void SysGameStart(DuelState state) {
        SysDuelInit(state);

        var data = new DataTable() {
            ["TurnPlayer"] = state.FirstPlayer,
            ["ActionPoints"] = state.FirstPlayer.Level
        };

        // Allow the turn data to be overridden.
        var source = new Source();
        var trigger = new Trigger("GameStart:GetTurnPlayer", source, state, TriggerType.DataOverride);
        state.DataTrigger(trigger, ref data);

        // Get the 
        var player = data.Get<Player>("TurnPlayer");
        var actionPoints = data.Get<int>("ActionPoints");
        state.NewTurn(player ?? state.FirstPlayer);
        state.FirstPlayer.ActionPoints = actionPoints;

        TurnActions.StartTurn(state);
    }

    /// <summary>
    /// Initializes all the meta data into state data and informs the client.
    /// </summary>
    [ActionName(nameof(SysDuelInit))]
    public static void SysDuelInit(DuelState state) {

        SendMapping(state);
        SysInitPlayer(state.Player1);
        SysInitPlayer(state.Player2);

    }

    public static void SendMapping(DuelState state) {
        // TODO CLIENT: SendMapping
    }

    /// <summary>
    /// Initializes a player from their meta data.
    /// </summary>
    /// <param name="player"></param>
    [ActionName(nameof(SysInitPlayer))]
    public static void SysInitPlayer(Player player) {
        var meta = player.Meta;
        SysInitHand(player, meta.Hand);
        SysInitField(player, meta.Field);
        SysInitGrave(player, meta.Grave);
    }

    /// <summary>
    /// Initializes a player's hand from their meta data.
    /// </summary>
    private static void SysInitHand(Player player, ICollection<CardMeta> handMeta) {
        foreach (var cardMeta in handMeta) {
            CardInstance card = new(player, cardMeta);
            if (cardMeta.Values.TryGetValue("FixedLevel", out object? levelUnknown) && levelUnknown is int level) {
                card.FixedLevel = level;
            }
            player.AddCardToHand(card);
            // TODO CLIENT: add card to hand client.
        }
    }


    /// <summary>
    /// Initializes a player's grave from their meta data.
    /// </summary>
    private static void SysInitGrave(Player player, ICollection<CardMeta> graveMeta) {
        foreach (var cardMeta in graveMeta) {
            CardInstance card = new(player, cardMeta);
            if (cardMeta.Values.TryGetValue("FixedLevel", out object? levelUnknown) && levelUnknown is int level) {
                card.FixedLevel = level;
            }
            player.AddCardToGrave(card);
            // TODO CLIENT: add card to grave client.
        }
    }

    /// <summary>
    /// Initializes a player's field from their meta data.
    /// </summary>
    private static void SysInitField(Player player, IReadOnlyDictionary<int, CardMeta> fieldMeta) {
        var field = player.Field;
        foreach (var pair in fieldMeta) {
            int slot = pair.Key;
            // Ensure the slot is on the field.
            if (!field.IsValidPosition(slot)) continue;

            var cardMeta = pair.Value;
            CardInstance card = new(player, cardMeta);
            if (cardMeta.Values.TryGetValue("FixedLevel", out object? levelUnknown) && levelUnknown is int level) {
                card.FixedLevel = level;
            }

            // TODO: Use meta values.
            CardActions.SummonAsCreature(card, field[slot]);
        }
    }



}
