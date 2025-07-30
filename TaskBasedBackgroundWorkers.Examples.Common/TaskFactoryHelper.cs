using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public static class TaskFactoryHelper
    {
        public static TaskFactory CreateLongRunning(TaskScheduler scheduler)
        {
            return new TaskFactory(CancellationToken.None, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, scheduler);
        }
    }
}
