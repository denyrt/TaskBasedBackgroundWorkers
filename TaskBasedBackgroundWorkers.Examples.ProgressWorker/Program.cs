using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common;
using TaskBasedBackgroundWorkers.Examples.Common.Helpers;

namespace TaskBasedBackgroundWorkers.Examples.ProgressWorker
{
    public sealed class Program
    {
        public static void Main()
        {
            EmulationOfProgressTask();
        }

        private static void EmulationOfProgressTask()
        {
            using (var worker = new ProgressRelayTaskWorker(DoWorkAsync, TaskScheduler.Default, TaskCreationOptions.None))
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

        private static async Task DoWorkAsync(IProgress<int> progress, ProgressRelayTaskWorker worker, CancellationToken cancellationToken = default)
        {
            var random = new Random();
            var timeSpan = TimeSpan.FromMilliseconds(1500);
            int index = 0;
            int count = 10;

            while (index < count && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ConsoleHelper.LogToConsoleOutAsync($"(hash: {worker.GetHashCode()}) [do work {Guid.NewGuid():n}]");
                    
                    progress.Report(index);

                    if (random.Next(0, count) < index)
                    {
                        MethodThatThrowsExpectedException();
                    }
                    
                    if (index == count)
                    {
                        throw new Exception("Critical failure");
                    }
                }
                catch (NotSupportedException ex) 
                {
                    await ConsoleHelper.LogToConsoleOutAsync($"(hash: {worker.GetHashCode()}) [{ex.Message}]");
                }
                finally
                {
                    ++index;
                }

                await Task.Delay(timeSpan, cancellationToken);
            }
        }

        private static void MethodThatThrowsExpectedException()
        {
            throw new NotSupportedException("some handled excetion");
        }
    }
}
