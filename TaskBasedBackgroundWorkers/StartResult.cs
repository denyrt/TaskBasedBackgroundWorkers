namespace TaskBasedBackgroundWorkers
{
    /// <summary>
    /// A state that describes start operation result.
    /// </summary>
    public enum StartResult
    {
        None = 0,

        /// <summary>
        /// Worker is successfully started.
        /// </summary>
        Ok,

        /// <summary>
        /// Worker must be in stopped state to perform start.
        /// </summary>
        StopRequired,

        /// <summary>
        /// One of passed <see cref="System.Threading.CancellationToken"/> is already cancelled.
        /// </summary>
        AlreadyCancelled
    }
}
