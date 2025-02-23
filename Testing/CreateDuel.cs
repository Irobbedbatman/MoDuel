using MoDuel;
using MoDuel.Data;
using MoDuel.Players;
using MoDuel.Shared.Data;
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
        DuelSettingsLoaded settings = DuelSettingsLoaded.Load(new() {
            GameStartActionItemPath = "SysGameStart",
            GameEndActionItemPath = "SysGameEnd"
        }, content);

        if (settings == null) {
            Console.WriteLine("Failed to load settings.");
            throw new Exception("Error handling");
        }

        // Card meta the cards will use.
        CardMeta meta1 = new("Goon", []);
        CardMeta meta2 = new("Goon", []);

        // Define the cards that will start in the players hand.
        var hand = new List<CardMeta>() { meta1, meta1, meta1, meta2 };

        // Hero meta the heroes will use.
        HeroMeta heroMeta = new("HoodedFigure", []);

        // Create the meta for both players.
        PlayerMeta player1Meta = new("Player 1", heroMeta, ["Gold"], []) {
            HandCards = [.. hand]
        };
        PlayerMeta player2Meta = new("Player 2", heroMeta, ["Gold"], []) {
            HandCards = [.. hand]
        };

        // Load the meta data.
        var player1 = PlayerMetaLoaded.CreateFromRawData(player1Meta, content);
        var player2 = PlayerMetaLoaded.CreateFromRawData(player2Meta, content);


        // Create and return the duel state.
        return new(player1, player2, content, settings);
    }

}

#nullable enable
