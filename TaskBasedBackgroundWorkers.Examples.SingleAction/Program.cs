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
