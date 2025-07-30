using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.LongRunningLoop
{
    public sealed class LoopWorker : TaskWorker<int>
    {
        public LoopWorker(TaskFactory taskFactory) : base(taskFactory)
        {            
        }

        protected override async Task DoWorkAsync(IProgress<int> progress, CancellationToken cancellationToken)
        {
            int processed = 0;

            while (true) 
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress.Report(++processed);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);                
            }
        }
    }
}
