using System;

namespace TaskBasedBackgroundWorkers
{
    public sealed class TaskWorkerProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Value of current progrees.
        /// </summary>
        public int Progress { get; }

        /// <summary>
        /// Any data that could be associated with progress.
        /// </summary>
        public object Context { get; }

        public TaskWorkerProgressChangedEventArgs(int progress, object context = null)
        {
            Progress = progress;
            Context = context;
        }
    }
}
