using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskSchedule;

namespace Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TaskScheduleRegister.RegistTask(
                "timer", 
                () => Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                TimeSpan.FromSeconds(5));

            Console.ReadLine();
        }
    }
}
