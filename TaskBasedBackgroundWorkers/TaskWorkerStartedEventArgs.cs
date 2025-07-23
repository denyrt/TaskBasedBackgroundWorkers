namespace TaskBasedBackgroundWorkers
{
    public sealed class TaskWorkerStartedEventArgs : System.EventArgs
    {
        public new static readonly TaskWorkerStartedEventArgs Empty = new TaskWorkerStartedEventArgs();
    }
}
