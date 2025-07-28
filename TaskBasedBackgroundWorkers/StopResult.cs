namespace TaskBasedBackgroundWorkers
{
    /// <summary>
    /// A state that describes stop operation result.
    /// </summary>
    public enum StopResult
    {
        None = 0,

        /// <summary>
        /// Worker is successfully stopped.
        /// </summary>
        Ok,

        /// <summary>
        /// Worker must be in running state to perform stop.
        /// </summary>
        StartRequired
    }
}
