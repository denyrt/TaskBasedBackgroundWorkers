using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;
using TaskBasedBackgroundWorkers.Examples.Common.Helpers;

namespace TaskBasedBackgroundWorkers.Examples.LongRunningLoop
{
    public sealed class Program
    {
        public static void Main()
        {
            EmulationOfLongRunningLoopTask();
        }

        private static void EmulationOfLongRunningLoopTask()
        {
            using (var worker = new RelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.LongRunning))
            {
                worker.EnableConsoleOut();
                worker.Start();

                ConsoleHelper.ReadInputLine("Press <Enter> to stop worker...");

                if (worker.IsRunning)
                {
                    worker.Stop();
                }

                ConsoleHelper.ReadInputLine("Press <Enter> to exit...");
            }
        }

        private static async Task DoWorkAsync(RelayTaskWorker worker, CancellationToken cancellationToken = default)
        {
            var timeSpan = TimeSpan.FromSeconds(1);

            while (!cancellationToken.IsCancellationRequested)
            {
                await ConsoleHelper.LogToConsoleOutAsync($"(hash: {worker.GetHashCode()}) [do work {Guid.NewGuid():n}]");

                await Task.Delay(timeSpan, cancellationToken);
            }
        }
    }
}
