using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;
using TaskBasedBackgroundWorkers.Examples.Common.Helpers;

namespace TaskBasedBackgroundWorkers.Examples.SingleAction
{
    public sealed class Program
    {
        public static void Main()
        {
            EmulationOfSingleActionTask();
        }

        private static void EmulationOfSingleActionTask()
        {
            using (var worker = new RelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.None))
            {
                worker.EnableConsoleOut();
                worker.Start();

                ConsoleHelper.ReadInputLine("Press <Enter> to stop worker if it is still running...");

                if (worker.IsRunning)
                {
                    worker.Stop();
                }

                ConsoleHelper.ReadInputLine("Press <Enter> to exit...");
            }
        }

        private static async Task DoWorkAsync(RelayTaskWorker worker, CancellationToken cancellationToken = default)
        {
            var index = 0;
            var timeSpan = TimeSpan.FromSeconds(1);

            while (index < 5 && !cancellationToken.IsCancellationRequested)
            {
                await ConsoleHelper.LogToConsoleOutAsync($"(hash: {worker.GetHashCode()}) [do work {Guid.NewGuid():n}]");
                
                await Task.Delay(timeSpan, cancellationToken);

                ++index;
            }
        }
    }
}
