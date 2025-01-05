using MoDuel;
using MoDuel.Cards;
using MoDuel.Data;
using MoDuel.Heroes;
using MoDuel.Players;
using MoDuel.Resources;
using MoDuel.State;

namespace Testing;
#nullable disable

/// <summary>
/// Methods to build a <see cref="DuelState"/> ready to run from test data.
/// </summary>
public class CreateDuel {

    public static DuelState CreateState() {

        // Get the default package.
        string[] packages = [
            "../../../../DefaultPackage\\bin\\Debug\\net8.0\\Data\\DefaultPackage.json"
        ];


        LogSettings.LoggedEvents = LogSettings.LogEvents.LogAll;

        // Load the package catalogue.
        PackageCatalogue content = new(packages);

        // Define the game settings.
        DuelSettings settings = new() {
            GameStartAction = content.LoadAction("SysGameStart"),
            GameEndAction = content.LoadAction("SysGameEnd")
        };

        // Load Card data.
        CardMeta meta = new(content.LoadCard("Goon"));
        CardMeta meta2 = new(content.LoadCard("Mystic"));

        // Define the cards that will start in the players hand.
        var hand = new List<CardMeta>() { meta, meta, meta, meta2 };

        // Create the player resource pool.
        ResourceType resource = content.LoadResourceType("Gold");
        var pool = new ResourceType[] { resource };
        
        // Load the hero will use.
        Hero hero = content.LoadHero("HoodedFigure");

        // Create the meta for both players.
        PlayerMeta player1 = new("Player 1", pool, hero, hand, hand, new Dictionary<int, CardMeta>(), []);
        PlayerMeta player2 = new("Player 2", pool, hero, hand, hand, new Dictionary<int, CardMeta>(), []);

        // Create and return the duel state.
        return new(player1, player2, content, settings);
    }

}

#nullable enable
