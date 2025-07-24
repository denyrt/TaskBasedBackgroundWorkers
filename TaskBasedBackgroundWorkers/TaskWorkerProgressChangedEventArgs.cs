namespace TaskBasedBackgroundWorkers
{
    public class TaskWorkerProgressChangedEventArgs<TProgress> : System.EventArgs
    {
        /// <summary>
        /// Value of current progrees.
        /// </summary>
        public TProgress Value { get; }

        public TaskWorkerProgressChangedEventArgs(TProgress value)
        {
            Value = value;
        }
    }
}
