using System;

namespace PhaseSonar.Utils
{
    /// <summary>
    /// A stop watch used for timing.
    /// </summary>
    public class StopWatch
    {
        private DateTime _start;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public StopWatch()
        {
            _start = DateTime.Now;
        }

        /// <summary>
        /// Return the time elasped in seconds and reset the clock to start timing again
        /// </summary>
        /// <returns>The time elasped in seconds</returns>
        public double Reset()
        {
            var now = DateTime.Now;
            var elapsed = (now - _start).TotalSeconds;
            _start = now;
            return elapsed;
        }

        /// <summary>
        /// Return the time elasped in seconds from start.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>The time elasped in seconds from start.</returns>
        public double ElapsedSeconds(string tag = @"elapsed") {
            return (DateTime.Now - _start).TotalSeconds;
        }
    }
}