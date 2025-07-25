using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Helpers
{
    public static class TaskHelper
    {
        public static bool IsCouldBeDisposed(Task task)
        {
            return task.Status == TaskStatus.RanToCompletion
                || task.Status == TaskStatus.Faulted
                || task.Status == TaskStatus.Canceled;
        }
    }
}
