using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public sealed class RelayTaskWorker : TaskWorker<int>
    {
        private readonly Func<CancellationToken, Task> _doWork;

        public RelayTaskWorker(
            Func<CancellationToken, Task> doWork,
            TaskScheduler taskScheduler,
            TaskCreationOptions taskCreationOptions
        )
            : base(taskScheduler, taskCreationOptions)
        {
            _doWork = doWork;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await _doWork.Invoke(cancellationToken).ConfigureAwait(false);
        }
    }
}
