using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoDuel.Tools {
    public class PlaybackBlockingHandler : IDisposable {

        /// <summary>
        /// Check to see if there is currently an animation blocking happening.
        /// </summary>
        public bool IsRunning { get; private set; } = false;
        /// <summary>
        /// How long the current animation blocking has to play for before finishing.
        /// <para>Measured in seconds.</para>
        /// </summary>
        public double DurationRemaining => CurrentBlockDuration - (DateTime.UtcNow - CurrentBlock).TotalSeconds;
        /// <summary>
        /// The utc time the current playback blocking started.
        /// </summary>
        public DateTime CurrentBlock { get; private set; } = DateTime.UtcNow;
        /// <summary>
        /// The duration of the currently playing block. Use <see cref="DurationRemaining"/> to calculate how long the blocking has gone for.
        /// <para>Measured in seconds.</para>
        /// </summary>
        public double CurrentBlockDuration { get; private set; } = 0;
        /// <summary>
        /// Cancelation token  source used to abrubtly end the blocking.
        /// <para>Use <see cref="EndBlock"/> to use it.</para>
        /// </summary>
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Starts blocking the thread so that an animation can happen before any furthur flow on <see cref="DuelFlow"/>.
        /// </summary>
        /// <param name="blockTime">How long to block the thread for.</param>
        public bool StartBlock(double blockTime) {
            //Ensure there isn't already an animation playing.
            if (IsRunning)
                return false;
            //Update the current animation variables.
            CurrentBlock = DateTime.UtcNow;
            CurrentBlockDuration = blockTime;
            IsRunning = true;
            //Clear and recreate the token source so we can reuse it.
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            //Start a task that delays the current thread until the animation has stopped.
            Task t = Task.Delay((int)Math.Floor(CurrentBlockDuration * 1000.0), tokenSource.Token);
            try {
                t.Wait();
            }
            catch (AggregateException) {
                //Animation Cancelled
            }
            finally {
                //Notify others that the animation is no longer playing.
                IsRunning = false;
            }
            return true;
        }

        /// <summary>
        /// Forces the animation blocking to end by stopping the task it uses.
        /// </summary>
        public void EndBlock() {
            tokenSource.Cancel();
        }

        /// <inheritdoc/>
        public void Dispose() {
            tokenSource.Dispose();
        }
    }
}
