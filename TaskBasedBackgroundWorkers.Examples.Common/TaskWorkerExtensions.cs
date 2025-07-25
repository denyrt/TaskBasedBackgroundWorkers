namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public static class TaskWorkerExtensions
    {
        public static void EnableConsoleLog<T>(this TaskWorker<T> worker)
        {
            worker.Started += LogWorkerStarted;
            worker.Stopped += LogWorkerStopped;
            worker.ProgressChanged += LogWorkerProgressChanged;
            worker.ExceptionThrown += LogWorkerExceptionThrown;
        }

        private static void LogWorkerStarted(object sender, TaskWorkerStartedEventArgs e)
        {
            ConsoleExtensions.WriteLineTimestamped($"(hash: {sender.GetHashCode()}) worker started");
        }

        private static void LogWorkerStopped(object sender, TaskWorkerStoppedEventArgs e)
        {
            ConsoleExtensions.WriteLineTimestamped($"(hash: {sender.GetHashCode()}) worker stopped [StopReason: {e.StopReason}]");
        }

        private static void LogWorkerProgressChanged<T>(object sender, TaskWorkerProgressChangedEventArgs<T> e)
        {
            ConsoleExtensions.WriteLineTimestamped($"(hash: {sender.GetHashCode()}) worker progress [Value: {e.Value}]");
        }

        private static void LogWorkerExceptionThrown(object sender, TaskWorkerExceptionEventArgs e)
        {
            ConsoleExtensions.WriteLineTimestamped($"(hash: {sender.GetHashCode()}) worker unexpected exception: {e.Exception.Message}");
        }
    }
}
