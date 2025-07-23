using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Helpers
{
    public static class TaskHelper
    {
        public static bool IsCouldBeDisposed(Task task)
        {
            if (task == null)
            {
                return false;
            }

            return task.Status == TaskStatus.RanToCompletion
                || task.Status == TaskStatus.Faulted
                || task.Status == TaskStatus.Canceled;
        }
    }
}
