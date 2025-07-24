using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public abstract class ExampleTaskWorker : TaskWorker<int>
    {
        protected ExampleTaskWorker(
            TaskScheduler taskScheduler, 
            TaskCreationOptions taskCreationOptions
        ) 
            : base(taskScheduler, taskCreationOptions)
        {
        }
    }
}
