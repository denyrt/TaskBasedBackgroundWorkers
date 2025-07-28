using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Helpers
{
    internal static class TaskHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCouldBeDisposed(Task task)
        {
            return task.IsCompleted;
        }
    }
}
