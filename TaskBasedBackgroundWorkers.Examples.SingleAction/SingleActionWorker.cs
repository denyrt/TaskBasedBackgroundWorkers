using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.SingleAction
{
    public sealed class SingleActionWorker : TaskWorker<int>
    {
        protected override async Task DoWorkAsync(IProgress<int> progress, CancellationToken cancellationToken)
        {
            for (int i = 0; i < 5; ++i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress.Report(i);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
