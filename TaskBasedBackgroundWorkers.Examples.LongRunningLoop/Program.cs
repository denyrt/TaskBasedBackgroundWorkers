using System;
using System.Linq;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;

namespace TaskBasedBackgroundWorkers.Examples.LongRunningLoop
{
    public sealed class Program
    {
        public static async Task Main()
        {
            var taskFactory = TaskFactoryHelper.CreateLongRunning(TaskScheduler.Default);

            using (var worker = new LoopWorker(taskFactory))
            {
                worker.EnableConsoleLog();

                await Start(worker, TimeSpan.Zero);
                
                ConsoleExtensions.ReadEnter("Press <Enter> to stop worker...");

                if (worker.IsRunning)
                {
                    /* Runs before Disposing with running and awaiting each task sequally */

                    //var stopTasks = Enumerable
                    //    .Range(0, 5)
                    //    .Select(async x => await Stop(worker, delay: TimeSpan.FromMilliseconds(x * 10)));

                    /* Could run after Disposing + runs in parallel way (Task.WhenAll awaits only task-creation but not its execution)*/

                    var stopTasks = Enumerable
                        .Range(0, 5)
                        .Select(x => Task.Factory.StartNew(async () => await Stop(worker, delay: TimeSpan.FromMilliseconds(x * 50))));

                    await Task.WhenAll(stopTasks);
                }
            }

            ConsoleExtensions.ReadEnter("Press <Enter> to exit...");
        }

        private static async Task Start<T>(TaskWorker<T> worker, TimeSpan delay)
        {
            try
            {
                await Task.Delay(delay).ConfigureAwait(false);

                StartResult start = worker.Start();

                ConsoleExtensions.WriteLineTimestamped($"(hash: {worker.GetHashCode()}) <{nameof(worker.Start)}> called [start = {start}]");
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLineTimestamped($"(hash: {worker.GetHashCode()}) <{nameof(worker.Start)}> {ex.Message}");
            }
        }

        private static async Task Stop<T>(TaskWorker<T> worker, TimeSpan delay) 
        {
            try
            {
                await Task.Delay(delay).ConfigureAwait(false);

                StopResult stop = worker.Stop();

                ConsoleExtensions.WriteLineTimestamped($"(hash: {worker.GetHashCode()}) <{nameof(worker.Stop)}> called [stop = {stop}]");
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteLineTimestamped($"(hash: {worker.GetHashCode()}) <{nameof(worker.Stop)}> {ex.Message} \n {ex.StackTrace}");
            }
        }
    }
}
