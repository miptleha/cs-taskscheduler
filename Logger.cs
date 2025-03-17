using System;
using System.Threading;

namespace cs_taskscheduler
{
    public static class Logger
    {
        // Logs a message with a timestamp, thread ID, and additional information
        public static void Log(string message)
        {
            // Get the current timestamp in the format "HH:mm:ss.fff"
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            // Get the current thread ID
            int threadId = Thread.CurrentThread.ManagedThreadId;

            // Output the log message with timestamp, thread ID, and the provided message
            Console.WriteLine($"[{timestamp}] [Thread:{threadId}] {message}");
        }
    }
}