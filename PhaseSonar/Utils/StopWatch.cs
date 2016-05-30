using System;

namespace PhaseSonar.Utils
{
    public class StopWatch
    {
        private DateTime _start;

        public StopWatch()
        {
            _start = DateTime.Now;
        }

        public void Pause(string tag = @"elapsed")
        {
            Console.WriteLine(tag + @": " + (DateTime.Now - _start).TotalSeconds);
        }

        public double Reset(string tag = @"elapsed")
        {
            var now = DateTime.Now;
            var elapsed = (now - _start).TotalSeconds;
            _start = now;
            //Console.WriteLine(tag + @": " + elapsed);
            return elapsed;
        }
    }
}