using System;
using System.Threading.Tasks;

namespace TaskBasedBackgroundWorkers.Examples.Common.Helpers
{
    public static class ConsoleHelper
    {
        public static void LogToConsoleOut(FormattableString message)
        {
            Console.Out.WriteLine(TimestampedMessage(message).ToString());
        }

        public static Task LogToConsoleOutAsync(FormattableString message)
        {
            return Console.Out.WriteLineAsync(TimestampedMessage(message).ToString());
        }

        public static FormattableString TimestampedMessage(FormattableString message)
        {
            return $"[{DateTimeOffset.Now:T}]: {message}";
        }

        public static void ReadInputLine(string message)
        {
            Console.Out.WriteLine(message);
            Console.In.ReadLine();
        }
    }
}
