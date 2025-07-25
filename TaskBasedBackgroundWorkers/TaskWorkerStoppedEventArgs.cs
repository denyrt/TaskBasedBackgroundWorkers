namespace TaskBasedBackgroundWorkers
{
    public sealed class TaskWorkerStoppedEventArgs : System.EventArgs
    {
        /// <summary>
        /// A state that describes how worker was finished.
        /// </summary>
        public TaskWorkerStopReason StopReason { get; }

        public TaskWorkerStoppedEventArgs(TaskWorkerStopReason stopReason)
        {
            StopReason = stopReason;
        }

        /// <summary>
        /// A reserved instance that describes empty state of <see cref="TaskWorkerStoppedEventArgs"/>.
        /// </summary>
        /// <remarks>
        /// Notice that <see cref="StopReason"/> assigned to <see cref="TaskWorkerStopReason.None"/>.
        /// </remarks>
        public new static readonly TaskWorkerStoppedEventArgs Empty = new TaskWorkerStoppedEventArgs(TaskWorkerStopReason.None);

        /// <summary>
        /// An reserved instance that describes successful execution of task worker.
        /// </summary>
        public static readonly TaskWorkerStoppedEventArgs Finished = new TaskWorkerStoppedEventArgs(TaskWorkerStopReason.Finished);

        /// <summary>
        /// An reserved instance that describes forced stop of worker by cancellation request.
        /// </summary>
        public static readonly TaskWorkerStoppedEventArgs Cancelled = new TaskWorkerStoppedEventArgs(TaskWorkerStopReason.Cancelled);

        /// <summary>
        /// An reserved instance that describes stopping of worker due to unhandled exception.
        /// </summary>
        public static readonly TaskWorkerStoppedEventArgs Exception = new TaskWorkerStoppedEventArgs(TaskWorkerStopReason.Exception);
    }
}
