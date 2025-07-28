namespace TaskBasedBackgroundWorkers
{
    public enum TaskWorkerStopReason
    {
        None = 0,

        /// <summary>
        /// Task worker successfully finished execution.
        /// </summary>
        Finished,

        /// <summary>
        /// Task worker was stoped using cancellation request.
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Task worker fault with unhandled exception.
        /// </summary>
        Exception
    }
}
