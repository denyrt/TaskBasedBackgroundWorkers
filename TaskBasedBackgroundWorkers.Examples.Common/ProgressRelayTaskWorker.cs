using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    // ToDo: a bit refactor this example (not best way to impl relay worker).
    public sealed class ProgressRelayTaskWorker : ExampleTaskWorker
    {
        private readonly Func<IProgress<int>, ProgressRelayTaskWorker, CancellationToken, Task> _doWork;

        public ProgressRelayTaskWorker(
            Func<IProgress<int>, ProgressRelayTaskWorker, CancellationToken, Task> doWork,
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
                await _doWork.Invoke(progress, this, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                progress.ProgressChanged -= Progress_ProgressChanged;
                progress = null;
            }
        }

        private void Progress_ProgressChanged(object sender, int e)
        {
            OnProgressChanged(new TaskWorkerProgressChangedEventArgs<int>(e));
        }
    }
}
