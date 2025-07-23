using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public sealed class ProgressRelayTaskWorker : TaskWorker
    {
        private readonly Func<IProgress<int>, CancellationToken, Task> _doWork;

        public ProgressRelayTaskWorker(
            Func<IProgress<int>, CancellationToken, Task> doWork,
            TaskScheduler taskScheduler, 
            TaskCreationOptions taskCreationOptions
        ) 
            : base(taskScheduler, taskCreationOptions)
        {
            _doWork = doWork;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var progress = new Progress<int>();
            progress.ProgressChanged += Progress_ProgressChanged;

            try
            {
                await _doWork.Invoke(progress, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                progress.ProgressChanged -= Progress_ProgressChanged;
                progress = null;
            }
        }

        private void Progress_ProgressChanged(object sender, int e)
        {
            OnProgressChanged(new TaskWorkerProgressChangedEventArgs(e));
        }
    }
}
