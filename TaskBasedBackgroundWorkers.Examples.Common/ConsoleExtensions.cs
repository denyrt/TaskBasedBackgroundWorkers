using System;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common
{
    public static class ConsoleExtensions
    {
        public static void WriteLineTimestamped(FormattableString message)
        {
            Console.Out.WriteLine(TimestampedMessage(message).ToString());
        }

        public static Task WriteLineTimestampedAsync(FormattableString message)
        {
            return Console.Out.WriteLineAsync(TimestampedMessage(message).ToString());
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
