using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;
using TaskBasedBackgroundWorkers.Extensions;

namespace TaskBasedBackgroundWorkers.Examples.SingleAction
{
    public sealed class Program
    {
        private static readonly RelayTaskWorker worker = new RelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.None);

        public static void Main()
        {
            worker.Started += (s, e) => Console.WriteLine("Worker started!");
            worker.Stopped += (s, e) => Console.WriteLine("Worker stopped! [IsForced = {0}]", e.IsForcedStop);

            using (worker)
            {
                worker.Start();

                Console.WriteLine("Press <Enter> to stop worker if it is still running...");
                Console.ReadLine();

                if (worker.IsRunning)
                {
                    worker.Stop();
                }

                Console.WriteLine("Press <Enter> to exit...");
                Console.ReadLine();
            }
        }

        private static async Task DoWorkAsync(CancellationToken cancellationToken = default)
        {
            var i = 0;
            var t = TimeSpan.FromSeconds(1);

            while (i < 5 && !cancellationToken.IsCancellationRequested)
            {
                await Console.Out.WriteLineAsync($"{Guid.NewGuid():n} Hello, DoWork!");
                await Task.Delay(t, cancellationToken);

                ++i;
            }
        }
    }
}
