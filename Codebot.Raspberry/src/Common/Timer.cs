using System.Diagnostics;

namespace Codebot.Raspberry.Common
{
    public class Timer
    {
        private static readonly double frequency = Stopwatch.Frequency;
        private double start;

        public Timer()
        {
            Reset();
        }

        public void Reset()
        {
            start = Stopwatch.GetTimestamp();
        }

        public static double Now { get => Stopwatch.GetTimestamp() / frequency; }
        public double ElapsedSeconds { get => (Stopwatch.GetTimestamp() - start) / frequency; }
        public double ElapsedMilliseconds { get => ElapsedSeconds * 1000d; }
        public double ElapsedMicroseconds { get => ElapsedMilliseconds * 1000d; }
        public double ElapsedNanoseconds { get => ElapsedMicroseconds * 1000d; }
    }
}
