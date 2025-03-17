using System;
using System.Threading;

namespace cs_taskscheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxConcurrentTasks = 5; // Maximum number of concurrent tasks
            MainLoop mainLoop = new MainLoop(maxConcurrentTasks);
            mainLoop.Start();

            // Stop the MainLoop after 15 seconds
            Thread.Sleep(15000);
            mainLoop.Stop();

            // The program will exit automatically
        }
    }
}
