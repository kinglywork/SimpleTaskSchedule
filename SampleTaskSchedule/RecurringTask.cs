using System;

namespace TaskSchedule
{
    public class RecurringTask : ITask
    {
        public string TaskId { get; set; }

        public DateTime StartTime { get; set; }

        public Action TaskAction { get; set; }

        /// <summary>
        /// TimeSpan.Zero mean null
        /// </summary>
        public TimeSpan Recurrence { get; set; }
        
        public RecurringTask(Action taskAction, DateTime startTime, TimeSpan recurrence, string taskId = null)
        {
            TaskAction = taskAction;
            StartTime = startTime;
            Recurrence = recurrence;
            TaskId = taskId;
        }               

        public void Run()
        {
            TaskAction();
        }

        public DateTime GetNextRunTime(DateTime lastExecutionTime)
        {
            return Recurrence != TimeSpan.Zero ? lastExecutionTime.Add(Recurrence) : DateTime.MinValue;
        }
    }
}
