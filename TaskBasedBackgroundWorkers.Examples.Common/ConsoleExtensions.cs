using System;
using System.Threading;
using System.Threading.Tasks;
using TaskBasedBackgroundWorkers.Concurrency;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public static class ConsoleExtensions
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static void WriteLineTimestamped(FormattableString message)
        {
            using (var handle = ConcurrentHandle.EnterBlocking(_semaphore))
            {
                Console.Out.WriteLine(TimestampedMessage(message).ToString());
            }
        }

        public static async Task WriteLineTimestampedAsync(FormattableString message)
        {
            using (var handle = await ConcurrentHandle.EnterBlockingAsync(_semaphore))
            {
                await Console.Out.WriteLineAsync(TimestampedMessage(message).ToString());
            }
        }

        public static FormattableString TimestampedMessage(FormattableString message)
        {
            return $"[{DateTimeOffset.Now:T}]: {message}";
        }

        public static void ReadEnter(string message)
        {
            Console.Out.WriteLine(message);
            Console.In.ReadLine();
        }
    }
}
