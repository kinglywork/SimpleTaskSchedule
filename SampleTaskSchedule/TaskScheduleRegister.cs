using System;

namespace TaskSchedule
{
    public static class TaskScheduleRegister
    {
        private static TaskScheduler _taskScheduler;

        private static TaskScheduler TaskScheduler => _taskScheduler ?? (_taskScheduler = new TaskScheduler());
        
        public static void RegistTask(string taskId, Action action, TimeSpan interval)
        {
            var startTime = DateTime.Now.AddSeconds(10);
            RegistTask(taskId, action, interval, startTime);
        }

        public static void RegistTask(string taskId, Action action, TimeSpan interval, DateTime startTime)
        {
            if (!TaskScheduler.Started)
            {
                TaskScheduler.Start();
            }
            
            TaskScheduler.AddTask(new RecurringTask(action, startTime, interval));
        }
    }
}
