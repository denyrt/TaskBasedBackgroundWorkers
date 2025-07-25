using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Examples.Common.Handlers;

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

        public void EnableConsoleOut()
        {
            Started += LogToConsoleOutHandlers.LogWorkerStarted;
            Stopped += LogToConsoleOutHandlers.LogWorkerStopped;
            ProgressChanged += LogToConsoleOutHandlers.LogWorkerProgressChanged;
            ExceptionThrown += LogToConsoleOutHandlers.LogWorkerExceptionThrown;
        }
    }
}
