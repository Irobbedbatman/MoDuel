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

        [MoonSharpHidden]
        public void EnqueueCommand(string cmdId, Player player, params object[] args) {
            CommandQueue.Enqueue (() =>
                DoAction(cmdId, args.ToList().Prepend(player).ToArray())
            );
            ResetTimer();
            ContinueEvent.Set();
        }

        public void ResetTimer() {
            if (Environment.Settings.TimeOutPlayers)
                TimeOutTimer.Start();
        }

    }
}
