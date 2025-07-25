using TaskBasedBackgroundWorkers.Examples.Common.Helpers;

namespace TaskBasedBackgroundWorkers.Examples.Common.Handlers
{
    public static class LogToConsoleOutHandlers
    {
        public static void LogWorkerStarted(object sender, TaskWorkerStartedEventArgs e)
        {
            ConsoleHelper.LogToConsoleOut($"(hash: {sender.GetHashCode()}) worker started");
        }

        public static void LogWorkerStopped(object sender, TaskWorkerStoppedEventArgs e)
        {
            ConsoleHelper.LogToConsoleOut($"(hash: {sender.GetHashCode()}) worker stopped [StopReason: {e.StopReason}]");
        }

        public static void LogWorkerProgressChanged<T>(object sender, TaskWorkerProgressChangedEventArgs<T> e)
        {
            ConsoleHelper.LogToConsoleOut($"(hash: {sender.GetHashCode()}) worker progress [Value: {e.Value}]");
        }

        public static void LogWorkerExceptionThrown(object sender, TaskWorkerExceptionEventArgs e) 
        {
            ConsoleHelper.LogToConsoleOut($"(hash: {sender.GetHashCode()}) worker unexpected exception: {e.Exception.Message}");
        }
    }
}
