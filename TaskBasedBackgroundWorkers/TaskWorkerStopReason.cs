namespace TaskBasedBackgroundWorkers
{
    public enum TaskWorkerStopReason
    {
        None = 0,

        /// <summary>
        /// Task worker successfully finished execution.
        /// </summary>
        Finished = 1,

        /// <summary>
        /// Task worker was cancelled using external <see cref="System.Threading.CancellationTokenSource"/> or <see cref="TaskWorker{TProgress}.Stop"/> 
        /// </summary>
        Cancelled = 2,
        
        /// <summary>
        /// Task worker fault with unhandled exception.
        /// </summary>
        Exception = 3
    }
}
