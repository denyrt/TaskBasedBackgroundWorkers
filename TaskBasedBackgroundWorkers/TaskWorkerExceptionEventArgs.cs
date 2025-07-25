namespace TaskBasedBackgroundWorkers
{
    public sealed class TaskWorkerExceptionEventArgs : System.EventArgs
    {
        public System.Exception Exception { get; }

        public TaskWorkerExceptionEventArgs(System.Exception exception)
        {
            Exception = exception;
        }
    }
}
