using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers
{
    public sealed class RelayTaskWorker : TaskWorker
    {
        private readonly Func<CancellationToken, Task> _doWorkAction;

        public RelayTaskWorker(
            Func<CancellationToken, Task>   doWorkAction,
            TaskScheduler                   taskScheduler, 
            TaskCreationOptions             taskCreationOptions
        ) 
            : base(taskScheduler, taskCreationOptions)
        {
            _doWorkAction = doWorkAction;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await _doWorkAction.Invoke(cancellationToken).ConfigureAwait(false);
        }
    }
}
