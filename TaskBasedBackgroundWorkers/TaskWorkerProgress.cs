using System;

namespace TaskBasedBackgroundWorkers
{
    internal sealed class TaskWorkerProgress<TProgress> : IProgress<TProgress>, IDisposable
    {
        private Action<TaskWorkerProgressChangedEventArgs<TProgress>> Progress;

        private TaskWorkerProgress(Action<TaskWorkerProgressChangedEventArgs<TProgress>> handler)
        {
            Progress = handler;
        }

        public void Report(TProgress value)
        {
            Progress.Invoke(new TaskWorkerProgressChangedEventArgs<TProgress>(value));
        }

        public void Dispose()
        {
            Progress = null;
        }

        public static TaskWorkerProgress<TProgress> FromHandler(Action<TaskWorkerProgressChangedEventArgs<TProgress>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return new TaskWorkerProgress<TProgress>(handler);
        }
    }
}
