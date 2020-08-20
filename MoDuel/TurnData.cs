using MoonSharp.Interpreter;
using System;

namespace MoDuel {

    /// <summary>
    /// The values of a given turn, created for each new turn with whichever player's turn it is. 
    /// </summary>
    [MoonSharpUserData]
    public class TurnData {

        /// <summary>
        /// The player who is in control of the current turn.
        /// </summary>
        public readonly Player TurnOwner;
        /// <summary>
        /// How many actions the player can make this turn.
        /// </summary>
        public int ActionPoints { get; set; }
        /// <summary>
        /// The date and time this turn started.
        /// </summary>
        public readonly DateTime TimeTurnStarted;
        /// <summary>
        /// How long this turn has been going for.
        /// </summary>
        public TimeSpan TimeElapsed => DateTime.UtcNow - TimeTurnStarted;

        public TurnData(Player turnOwner) {
            TurnOwner = turnOwner;
            ActionPoints = turnOwner.Level;
            TimeTurnStarted = DateTime.UtcNow;
        }

    }


}
