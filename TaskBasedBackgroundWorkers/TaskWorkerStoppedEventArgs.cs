namespace TaskBasedBackgroundWorkers
{
    public sealed class TaskWorkerStoppedEventArgs : System.EventArgs
    {
        /// <summary>
        /// A state that describes how worker was finished.
        /// </summary>
        /// <remarks>
        /// You usually want procudes something like 'Finished' if value is <see langword="false"/> otherwise produce something like 'Cancelled'.
        /// </remarks>
        public bool IsForcedStop { get; }

        public TaskWorkerStoppedEventArgs(bool isForceStop)
        {
            IsForcedStop = isForceStop;
        }

        public new static readonly TaskWorkerStoppedEventArgs Empty = new TaskWorkerStoppedEventArgs(isForceStop: false);
        
        public static readonly TaskWorkerStoppedEventArgs ForcedStop = new TaskWorkerStoppedEventArgs(isForceStop: true);
    }
}
