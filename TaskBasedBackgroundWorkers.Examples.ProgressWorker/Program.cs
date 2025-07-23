using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;
using TaskBasedBackgroundWorkers.Extensions;

namespace TaskBasedBackgroundWorkers.Examples.ProgressWorker
{
    public sealed class Program
    {
        private static readonly ProgressRelayTaskWorker taskWorker = new ProgressRelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.None);

        public static void Main()
        {
            taskWorker.ProgressChanged += (s, e) => Console.WriteLine($"Work done for a '{e.Progress}' points.");
            taskWorker.Started += (s, e) => Console.WriteLine("Worker started!");
            taskWorker.Stopped += (s, e) => Console.WriteLine("Worker stopped!");

            using (taskWorker)
            {
                taskWorker.Start();

                Console.WriteLine("Press <Enter> to stop worker...");
                Console.ReadLine();

                if (taskWorker.IsRunning)
                {
                    taskWorker.Stop();
                }

                Console.WriteLine("Press <Enter> to exit...");
                Console.ReadLine();
            }
        }

        private static async Task DoWorkAsync(IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            var t = TimeSpan.FromMilliseconds(500);

            for (int i = 0; i < 10; i++)
            {
                await Console.Out.WriteLineAsync($"{Guid.NewGuid():n} calculated");

                progress.Report(i + 1);

                await Task.Delay(t);
            }
        }
    }
}
