using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;
using TaskBasedBackgroundWorkers.Extensions;

namespace TaskBasedBackgroundWorkers.Examples.LongRunningLoop
{
    public sealed class Program
    {
        private static readonly RelayTaskWorker taskWorker = new RelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.LongRunning);

        public static void Main()
        {
            taskWorker.Started += (s, e) => Console.WriteLine("Long running worker started!");
            taskWorker.Stopped += (s, e) => Console.WriteLine("Long running worker stopped! [IsForced = {0}]", e.IsForcedStop);
            
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

        private static async Task DoWorkAsync(CancellationToken cancellationToken = default)
        {
            var t = TimeSpan.FromSeconds(1);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Console.Out.WriteLineAsync($"{Guid.NewGuid():n} Do Work Till Cancel...");
                await Task.Delay(t, cancellationToken);
            }
        }
    }
}
