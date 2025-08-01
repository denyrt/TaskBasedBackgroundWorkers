﻿using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;

namespace TaskBasedBackgroundWorkers.Examples.SingleAction
{
    public sealed class Program
    {
        public static void Main()
        {
            using (var worker = new SingleActionWorker(Task.Factory))
            using (var cts = new CancellationTokenSource())
            {
                worker.EnableConsoleLog();

                cts.Cancel();

                StartResult start = worker.Start(cts.Token);

                ConsoleExtensions.WriteLineTimestamped($"(hash: {worker.GetHashCode()}) <{nameof(worker.Start)}> called [start = {start}]");

                ConsoleExtensions.ReadEnter("Press <Enter> to stop worker...");

                if (worker.IsRunning)
                {
                    StopResult stop = worker.Stop();

                    ConsoleExtensions.WriteLineTimestamped($"(hash: {worker.GetHashCode()}) <{nameof(worker.Stop)}> called [stop = {stop}]");
                }
            }

            ConsoleExtensions.ReadEnter("Press <Enter> to exit...");
        }
    }
}
