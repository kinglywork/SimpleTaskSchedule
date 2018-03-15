using System;
using System.Linq;
using System.Threading;

namespace TaskSchedule
{
    /// <inheritdoc />
    /// <summary>
    /// Features: Fast, Small, Doens't poll, Recurring Tasks
    /// </summary>
    public class TaskScheduler : IDisposable
    {
        private readonly TaskCollection _taskQueue;

        private readonly AutoResetEvent _autoResetEvent;
        private Thread _thread;

        /// <summary>
        /// Is already started
        /// </summary>
        public bool Started { get; private set; }

        public TaskScheduler()
        {
            _taskQueue = new TaskCollection();
            _autoResetEvent = new AutoResetEvent(false);
        }

        /// <summary>
        /// Start running tasks
        /// </summary>
        public void Start()
        {
            lock (_taskQueue)
            {
                if (Started) return;

                Started = true;
                _thread = new Thread(Run) {IsBackground = true};
                _thread.Start();
            }
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            DebugLog("Task Scheduler thread stopping");
            Started = false;
            _autoResetEvent.Set();
            DebugLog("AutoResetEvent set called");
            _thread.Join();
            DebugLog("Task Scheduler thread stopped");
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Stop();
            _autoResetEvent.Dispose();
        }

        /// <summary>
        /// AddTask
        /// </summary>
        /// <param name="task">Once ITask object is added, it should never be updated from outside TaskScheduler</param>
        public void AddTask(ITask task)
        {
            ITask earliestTask;

            lock (_taskQueue)
            {
                earliestTask = GetEarliestScheduledTask();
                _taskQueue.Add(task);
            }
            DebugLog("Added task # " + task.TaskId);

            if (earliestTask != null && task.StartTime >= earliestTask.StartTime) return;

            _autoResetEvent.Set();
            DebugLog("AutoResetEvent is Set");
        }

        public void AddTask(Action taskAction, DateTime startTime)
        {
            AddTask(new RecurringTask(taskAction, startTime, TimeSpan.Zero));
        }

        public void AddTask(Action taskAction, DateTime startTime, TimeSpan recurrence)
        {
            AddTask(new RecurringTask(taskAction, startTime, recurrence));
        }

        private void ReScheduleRecurringTask(ITask task)
        {
            var nextRunTime = task.GetNextRunTime(task.StartTime);
            if (nextRunTime == DateTime.MinValue) return;
            task.StartTime = nextRunTime;
            lock (_taskQueue)
                _taskQueue.Add(task);
            DebugLog("Recurring task # " + task.TaskId + " scheduled for " + task.StartTime);
        }

        private ITask GetEarliestScheduledTask()
        {
            lock (_taskQueue)
            {
                return _taskQueue.First();
            }
        }

        public int TaskCount => _taskQueue.Count;

        public bool RemoveTask(ITask task)
        {
            DebugLog("Removing task # " + task.TaskId);
            lock (_taskQueue)
                return _taskQueue.Remove(task);
        }

        public bool RemoveTask(string taskId)
        {
            lock (_taskQueue)
                return _taskQueue.Remove(_taskQueue.First(n => n.TaskId == taskId));
        }

        public bool UpdateTask(ITask task, DateTime startTime)
        {
            DebugLog("Updating task # " + task.TaskId);
            lock (_taskQueue)
            {
                if (!RemoveTask(task)) return false;
                task.StartTime = startTime;
                AddTask(task);
                return true;
            }
        }

        private void Run()
        {
            DebugLog("Task Scheduler thread starting");
            var tolerance = TimeSpan.FromSeconds(1);
            while (Started)
            {
                try
                {
                    var task = GetEarliestScheduledTask();
                    if (task != null)
                    {
                        if (task.StartTime - DateTime.Now < tolerance)
                        {
                            DebugLog("Starting task " + task.TaskId);
                            try
                            {
                                task.Run();
                            }
                            catch (Exception e)
                            {
                                ErrorLog(e, "Exception while running Task # " + task.TaskId);
                            }
                            DebugLog("Completed task " + task.TaskId);

                            lock (_taskQueue) _taskQueue.Remove(task);
                            ReScheduleRecurringTask(task);
                        }
                        else
                        {
                            var waitTime = (task.StartTime - DateTime.Now);

                            DebugLog("Scheduler thread waiting for " + waitTime);
                            _autoResetEvent.WaitOne(waitTime);
                            DebugLog("Scheduler thread awakening from sleep " + waitTime);
                        }
                    }
                    else
                    {
                        DebugLog("Scheduler thread waiting indefinitely");
                        _autoResetEvent.WaitOne();
                        DebugLog("Scheduler thread awakening from indefinite sleep");
                    }
                }
                catch (Exception e)
                {
                    ErrorLog(e, "Error occurs in task schedule");
                }
            }

        }

        private static void DebugLog(string message)
        {
            Console.WriteLine(message);
        }

        private static void ErrorLog(Exception e, string message)
        {
            Console.WriteLine($"{message}, {e.Message}");
        }
    }
}
