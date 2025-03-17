using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cs_taskscheduler
{
    public class MainLoop
    {
        private SemaphoreSlim _semaphore; // Limits the number of concurrent tasks
        private Random _random; // Generates random numbers for task duration and task count
        private bool _isRunning; // Controls the main loop execution
        private int _maxConcurrentTasks; // Maximum number of concurrent tasks
        private Thread _mainLoopThread; // Thread for the main loop
        private static int _taskCounter = 0; // Counter for task numbering
        private bool[] _slotOccupied; // Tracks which slots are available

        public MainLoop(int maxConcurrentTasks)
        {
            _maxConcurrentTasks = maxConcurrentTasks;
            _semaphore = new SemaphoreSlim(maxConcurrentTasks); // Initialize semaphore with max slots
            _random = new Random();
            _isRunning = false;
            _slotOccupied = new bool[maxConcurrentTasks]; // Initialize slot availability tracker
        }

        // Starts the main loop in a separate thread
        public void Start()
        {
            _isRunning = true;
            _mainLoopThread = new Thread(RunMainLoop);
            _mainLoopThread.Start();
            Logger.Log($"MainLoop started. Max slots for tasks: {_maxConcurrentTasks}.");
        }

        // The main loop that checks and adds tasks every second
        private void RunMainLoop()
        {
            while (_isRunning)
            {
                CheckAndAddTasks();
                Thread.Sleep(1000); // Pause for 1 second
            }
        }

        // Stops the main loop and waits for all tasks to complete
        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                Logger.Log("MainLoop stopping... Waiting for all tasks to complete.");

                // Wait for the main loop thread to finish
                _mainLoopThread.Join();

                // Wait for all semaphore slots to be released
                for (int i = 0; i < _maxConcurrentTasks; i++)
                {
                    _semaphore.Wait(); // Block until all slots are free
                }

                // Release all slots to reset the semaphore
                _semaphore.Release(_maxConcurrentTasks);

                Logger.Log("MainLoop stopped. All tasks completed.");
            }
        }

        // Checks available slots and adds tasks if possible
        private void CheckAndAddTasks()
        {
            if (!_isRunning) return; // Exit if the main loop is stopped

            int availableSlots = _semaphore.CurrentCount; // Number of available slots
            if (availableSlots > 0)
            {
                // Randomly decide how many tasks to add (1 to available slots)
                int tasksToAdd = _random.Next(1, availableSlots + 1);
                Logger.Log($"Adding {tasksToAdd} tasks.");

                for (int i = 0; i < tasksToAdd; i++)
                {
                    _semaphore.Wait(); // Occupy a slot
                    int slotNumber = GetNextAvailableSlot(); // Get the first available slot
                    _slotOccupied[slotNumber - 1] = true; // Mark the slot as occupied (adjust for 0-based index)
                    int taskId = Interlocked.Increment(ref _taskCounter); // Generate a unique task ID

                    // Create a TaskData object with slot number, task ID, and additional parameters
                    var taskData = new TaskData(
                        slotNumber,
                        taskId,
                        $"Parameters for Task {taskId}" // Example of additional parameters
                    );

                    Task.Factory.StartNew(() => DoWork(taskData)); // Start a task with the TaskData object
                }
            }
            else
            {
                Logger.Log("No available slots");
            }
        }

        public class TaskData
        {
            public int SlotNumber { get; set; } // Slot number (1-based)
            public int TaskId { get; set; } // Task ID
            public string Parameters { get; set; } // Additional task parameters (example)

            public TaskData(int slotNumber, int taskId, string parameters)
            {
                SlotNumber = slotNumber;
                TaskId = taskId;
                Parameters = parameters;
            }
        }

        // Simulates task execution
        private void DoWork(TaskData taskData)
        {
            Logger.Log($"Task {taskData.TaskId} in slot {taskData.SlotNumber} started.");
            Thread.Sleep(_random.Next(1000, 10001)); // Simulate work (1 to 10 seconds)
            Logger.Log($"Task {taskData.TaskId} in slot {taskData.SlotNumber} completed.");
            _slotOccupied[taskData.SlotNumber - 1] = false; // Mark the slot as available (adjust for 0-based index)
            _semaphore.Release(); // Release the slot
        }

        // Finds the first available slot (1-based numbering)
        private int GetNextAvailableSlot()
        {
            for (int i = 0; i < _maxConcurrentTasks; i++)
            {
                if (!_slotOccupied[i])
                {
                    return i + 1; // Return the first available slot (1-based)
                }
            }
            throw new InvalidOperationException("No available slots found."); // This should never happen
        }
    }
}