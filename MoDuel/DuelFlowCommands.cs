using MoDuel.Tools;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoDuel {

    /// <summary>
    /// Handles the commands of <see cref="DuelFlow"/>
    /// <para>DuelFlow.cs is the main file and may need to be explored to understand the workings here.</para>
    /// </summary>
    public partial class DuelFlow {

        /// <summary>
        /// The list of commands buffered by each player.
        /// <para>Sorted by the time they were added.</para>
        /// <para>Only one command should be stored per <see cref="Player"/></para>
        /// </summary>
        [MoonSharpHidden]
        private readonly SortedList<CommandRefrence, Action> CommandBuffer = new SortedList<CommandRefrence, Action>();

        [MoonSharpHidden]
        public void EnqueueCommand(string cmdId, Player player, params object[] args) {

            // The time this command was added.
            DateTime commandTime = DateTime.UtcNow;

            // The wrapped the function we use as command, called in duelflow when neccasary.
            void command() {
                var span = DateTime.UtcNow - commandTime;
                // If the command was created more than a second before excution we ignore it.
                if (span.TotalSeconds > 1)
                    return;
                lock (ThreadLock) {
                    DoAction(cmdId, args.ToList().Prepend(player).ToArray());
                }
            }

            // Lock commands so that remove of commands is still thread safe.
            lock (CommandBuffer) {
                foreach (var cmd in CommandBuffer) {
                    if (cmd.Key.Player == player)
                        CommandBuffer.Remove(cmd.Key);
                }
                var cmdRef = new CommandRefrence() { Player = player, Time = commandTime };
                CommandBuffer[cmdRef] = command;
            }

            // Tell the flow to resume.
            ContinueEvent.Set();
        }

    }
}
