using MoonSharp.Interpreter;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace MoDuel {

    /// <summary>
    /// Handles the commands of <see cref="DuelFlow"/>
    /// <para>DuelFlow.cs is the main file and may need to be explored to understand the workings here.</para>
    /// </summary>
    public partial class DuelFlow {

        [MoonSharpHidden]
        private readonly ConcurrentQueue<Action> CommandQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// Removes all the commands from the command queue.
        /// </summary>
        public void ClearCommandQueue() {
            while (!CommandQueue.IsEmpty)
                CommandQueue.TryDequeue(out var _);
        }

        [MoonSharpHidden]
        public void EnqueueCommand(string cmdId, Player player, params object[] args) {
            //TODO: Only keep relevant commands.
            CommandQueue.Enqueue (() => {
                lock (ThreadLock) {
                    DoAction(cmdId, args.ToList().Prepend(player).ToArray());
                }
            });
            ContinueEvent.Set();
        }

    }
}
