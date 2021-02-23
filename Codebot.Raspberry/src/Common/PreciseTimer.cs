using System.Diagnostics;

namespace Codebot.Raspberry.Common
{
    /// <summary>
    /// The precise timer class is used to measure time intervals in with the
    /// greatest possible accuracy.
    /// </summary>
    public class PreciseTimer
    {
        private static readonly double frequency = Stopwatch.Frequency;
        private double start;

        public PreciseTimer()
        {
            Reset();
        }

        /// <summary>
        /// Reset the timer to zero.
        /// </summary>
        public void Reset()
        {
            start = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// The absolute number of milliseconds passed since the start of 
        /// the epoch.
        /// </summary>
        public static double Now { get => Stopwatch.GetTimestamp() / frequency * 1000d; }

        /// <summary>
        /// The number of seconds since the last reset.
        /// </summary>
        public double ElapsedSeconds { get => (Stopwatch.GetTimestamp() - start) / frequency; }

        /// <summary>
        /// The number of milliseconds since the last reset.
        /// </summary>
        public double ElapsedMilliseconds { get => ElapsedSeconds * 1000d; }

        /// <summary>
        /// The number of microseconds since the last reset.
        /// </summary>
        public double ElapsedMicroseconds { get => ElapsedMilliseconds * 1000d; }

        /// <summary>
        /// The number of nanoseconds since the last reset.
        /// </summary>
        public double ElapsedNanoseconds { get => ElapsedMicroseconds * 1000d; }
    }
}
