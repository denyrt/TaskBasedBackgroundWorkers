using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;

namespace TaskBasedBackgroundWorkers.Examples.ProgressWorker
{
    public sealed class Program
    {
        private static readonly ProgressRelayTaskWorker taskWorker = new ProgressRelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.None);

        public static void Main()
        {
            taskWorker.ProgressChanged += (s, e) =>
            {
                Console.WriteLine("Work done for a '{0}' points.", e.Value);
            };

            taskWorker.ExceptionThrown += (s, e) =>
            {
                Console.WriteLine("Worker failed with exception: {0}", e.Exception);
            };

            taskWorker.Started += (s, e) =>
            {
                Console.WriteLine("Worker started!");
            };

            taskWorker.Stopped += (s, e) =>
            {
                Console.WriteLine("Worker stopped! [IsForced = {0}]", e.IsForcedStop);
            };
            

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
            int i = 0;

            while (i < 10 && !cancellationToken.IsCancellationRequested)
            {
                await Console.Out.WriteLineAsync($"{Guid.NewGuid():n} calculated");

                progress.Report(++i);

                await Task.Delay(t, cancellationToken);

                throw new Exception("ExampleException.");
            }
        }
    }
}
