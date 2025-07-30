using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public static class ConsoleExtensions
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public static void WriteLineTimestamped(FormattableString message)
        {
            _semaphoreSlim.Wait();

            try
            {
                Console.Out.WriteLine(TimestampedMessage(message).ToString());
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public static async Task WriteLineTimestampedAsync(FormattableString message)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                await Console.Out.WriteLineAsync(TimestampedMessage(message).ToString());
            }
            finally
            {
                _semaphoreSlim.Release();
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
