using TaskBasedBackgroundWorkers.Examples.Common;

namespace TaskBasedBackgroundWorkers.Examples.LongRunningLoop
{
    public sealed class Program
    {
        public static void Main()
        {
            using (var worker = new LoopWorker())
            {
                worker.EnableConsoleLog();
                worker.Start();

                ConsoleExtensions.ReadEnter("Press <Enter> to stop worker...");

                if (worker.IsRunning)
                {
                    worker.Stop();
                }

                ConsoleExtensions.ReadEnter("Press <Enter> to exit...");
            }
        }
    }
}
