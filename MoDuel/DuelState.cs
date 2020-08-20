using MoDuel.Field;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDuel {

    /// <summary>
    /// Exception that is thrown when trying to check for a player, that has no refrence in this game.
    /// </summary>
    public class PlayerNotPlayingException : Exception {
        public PlayerNotPlayingException(string message) : base(message) { }
    }

    [MoonSharpUserData]
    public class DuelState {

        /// <summary>
        /// The player who created the game and is considered "near" by a <see cref="FullField"/>
        /// </summary>
        public readonly Player Player1;
        /// <summary>
        /// The player who joined the game and is considered "far" by a <see cref="FullField"/>
        /// </summary>
        public readonly Player Player2;

        /// <summary>
        /// The combination <see cref="Field"/ > of <see cref="SubField"/>s owned <see cref="Player1"/> and <seealso cref="Player2"/>
        /// </summary>
        public readonly FullField Field;

        public bool OnGoing = false;
        public readonly DuelSettings Settings;

        /// <summary>
        /// The data of any given turn, new() ones should be created each turn with the <see cref="Player"/> that's turn it is.
        /// </summary>
        public TurnData CurrentTurn { get; set; }

        /// <summary>
        /// The amount of turns that have passed this game.
        /// </summary>
        public int TurnCount = 1;

        public DuelState(Player nearPlayer, Player farPlayer, DuelSettings settings) {
            Player1 = nearPlayer;
            Player2 = farPlayer;
            Field = new FullField(nearPlayer.Field, farPlayer.Field);
            Settings = settings;
        }

        /// <summary>
        /// Gets the player from their <see cref="Player.UserId"/>.
        /// </summary>
        /// <param name="userId">The <see cref="Player.UserId"/> of a player to check.</param>
        /// <exception cref="PlayerNotPlayingException">Thrown when the <see cref="Player"/> is neither <see cref="Player1"/> or <seealso cref="Player2"/></exception>
        public Player GetPlayerByUserID(string userId) {
            if (Player1.UserId == userId)
                return Player1;
            else if (Player2.UserId == userId)
                return Player2;
            else
                throw new PlayerNotPlayingException("GetPlayerByUserID:: " + userId + " is not in game state.");
        }

        /// <summary>
        /// Gets the other player to the provided player.
        /// </summary>
        /// <exception cref="PlayerNotPlayingException">Thrown when the <see cref="Player"/> is neither <see cref="Player1"/> or <seealso cref="Player2"/></exception>
        public Player GetOpposingPlayer(Player player) {
            if (player == Player1)
                return Player2;
            else if (player == Player2)
                return Player1;
            else
                throw new PlayerNotPlayingException("GetOpposingPlayer:: " + player.UserId + " is not in game state");
        }

    }


}
