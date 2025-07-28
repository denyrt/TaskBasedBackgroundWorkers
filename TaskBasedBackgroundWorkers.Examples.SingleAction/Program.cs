using TaskBasedBackgroundWorkers.Examples.Common;

namespace TaskBasedBackgroundWorkers.Examples.SingleAction
{
    public sealed class Program
    {
        public static void Main()
        {
            using (var worker = new SingleActionWorker())
            {
                worker.EnableConsoleLog();
                
                StartResult start = worker.Start();

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
