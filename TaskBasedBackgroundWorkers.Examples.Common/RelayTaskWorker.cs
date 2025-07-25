using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    // ToDo: a bit refactor this example (not best way to impl relay worker).
    public sealed class RelayTaskWorker : ExampleTaskWorker
    {
        private readonly Func<RelayTaskWorker, CancellationToken, Task> _doWork;

        public RelayTaskWorker(
            Func<RelayTaskWorker, CancellationToken, Task> doWork,
            TaskScheduler taskScheduler,
            TaskCreationOptions taskCreationOptions
        )
            : base(taskScheduler, taskCreationOptions)
        {
            _doWork = doWork;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await _doWork.Invoke(this, cancellationToken).ConfigureAwait(false);
        }
    }
}
