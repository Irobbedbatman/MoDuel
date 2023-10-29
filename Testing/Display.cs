using MoDuel.Cards;
using MoDuel.Field;
using MoDuel.Players;
using MoDuel.Resources;

namespace Testing;

/// <summary>
/// A class that contain console display methods.
/// </summary>
public static class Display {

    /// <summary>
    /// Display the field, the creatures on the field and those creatures stats.
    /// </summary>
    public static void Field(Field field) {

        Console.Write("\n-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-\n|");
        // Creature info is display after the field and so we need to store it while we iterate the field.
        List<string> lines = new();

        foreach (var slot in field) {

            if (slot.IsOccupied) {
                // If there is a crature in the slot we provide a contrast.
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write($"[{slot.Index,3}]");
                Console.ResetColor();
                // Get the creature from the slot.
                var occ = slot.Occupant;
                // Display the creature's stats and which slot the creature is in..
                lines.Add($"{slot.Index}: {occ.Imprint.Id}, Lvl: {occ.Level}, Atk: {occ.Attack}, Def: {occ.Armour}, Life: {occ.Life}/{occ.MaxLife}\t@[{occ.Index}]");
                // Display the stored values of the creature in a table.
                if (occ.Values.Values.Count > 0) {
                    lines.Add("-- Values --");
                    foreach (var val in occ.Values.ToArray())
                        lines.Add(val.Key + ": " + val.Value);
                    lines.Add("------------");
                }
            }
            else {
                // If a slot is empty we just display the slot number.
                Console.Write($" {slot.Index,3} ");
            }
            // Display a sperator between field slots.
            Console.Write("|");
            // If we have finsished drawing the first row we display a sperator between the fields.
            if (field is FullField && field.SlotPosition(slot) == field.Count() / 2)
                Console.Write("\n-------------------------------\n|");
        }

        Console.WriteLine("\n-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");

        //IF the field itself has any values we can store them.
        if (field.Values.Values.Count > 0) {
            // FullField.Values is place you can store values for the whole game.
            lines.Add("-- Global Values --");
            foreach (var val in field.Values)
                lines.Add(val.Key + ": " + val.Value);
            lines.Add("------------");
        }

        // Display all the crature info we retreived early in the lines collection.
        if (lines.Count > 0) {
            foreach (var line in lines) {
                Console.WriteLine(line);
            }
            Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-");
        }

    }

    /// <summary>
    /// Display a collection of cards, primarily to display the hand and grave.
    /// </summary>
    /// <param name="collection"></param>
    public static void Collection(string collectionName, IEnumerable<CardInstance> collection) {
        Console.WriteLine("---" + collectionName + "-------");
        foreach (var card in collection) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{card.Imprint.Name,-30} @[{card.Index}]");
            Console.ResetColor();
            Console.WriteLine("Cost: " + card.GetDisplayedCost()?.ToString());
            Console.WriteLine($"Lvl: {card.GetDisplayedLevel() ?? "?"} Atk: {card.GetDisplayedAttack() ?? "?"} Def: {card.GetDisplayedArmour() ?? "?"} Life: {card.GetDisplayedLife() ?? "?"}");
            if (card.Values.Count > 0) {
                foreach (var val in card.Values)
                    Console.WriteLine(val.Key + ": " + val.Value);
            }
        }
        Console.Write("----------");
        for (int i = 0; i < collectionName.Length; ++i)
            Console.Write("-");
        Console.WriteLine();
    }

    /// <summary>
    /// The amount of experience it takes to go from the previous level to this level.
    /// </summary>
    public static readonly int LEVEL_2_XP = 6, LEVEL_3_XP = 30, INVALID_XP = -1;

    /// <summary>
    /// Retreive the value of exp required at a certain level to reach the next.
    /// </summary>
    public static int ExpNextLevel(int level) => level switch {
        1 => LEVEL_2_XP,
        2 => LEVEL_3_XP,
        _ => INVALID_XP,
    };

    /// <summary>
    /// Display all the player stats as neatly as possible.
    /// </summary>
    /// <param name="player"></param>
    public static void PlayerStats(Player player) {
        Console.WriteLine("_____________________");
        Console.WriteLine("`````````````````````");
        // Display the player's name and target index.
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(player.UserId);
        Console.ResetColor();
        Console.WriteLine("\t\t\t@[{0}]", player.Index);
        Console.Write("Hero: ");
        Console.ForegroundColor = ConsoleColor.Green;
        // Display what hero the player currently is using.
        Console.WriteLine(player.Hero.Imprint.Id);
        Console.ResetColor();
        Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-");
        // If the player is the turn player display the amount of action points.
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (player.IsTurnOwner())
            Console.WriteLine($"AP: {player.ActionPoints}");
        Console.ResetColor();
        // Display the player's life.
        Console.WriteLine("HP: {0} | {1}", player.Life, player.MaxLife);
        int nextExpLevel = ExpNextLevel(player.Level);
        // Display the player's exp.
        Console.WriteLine("Level: {0} ... Exp: {1}", player.Level, ((nextExpLevel == INVALID_XP) ? "---" : player.Exp + " | " + nextExpLevel.ToString()));
        // Display the player's resouces.
        foreach (ResourceCounter counter in player.ResourcePool) {
            // Print each resource name and how much the player has of that said resource.
            Console.WriteLine(counter.Name + ": " + counter.Count);
        }
        // Display all the misc values that have been stored on the player.
        if (player.Values.Keys.Count > 0) {
            Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-");
            foreach (var pair in player.Values) {
                Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
            }
        }
        Console.WriteLine("_____________________");
    }

}
