using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Codebot.Raspberry.Libc;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The precise timer class is used to measure time intervals in with the
    /// greatest possible accuracy.
    /// </summary>
    public class PreciseTimer : IDisposable
    {
        const double EPSILON = 0.000_000_1d;
        static readonly double frequency;
        timespec start;

        static PreciseTimer()
        {
            frequency = Stopwatch.Frequency;
            timespec t;
            t.tv_sec = IntPtr.Zero;
            t.tv_nsec = (IntPtr)10_000_1000;
            nanosleep(ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)1_000;
            for (var i =0; i < 10; i++)
                nanosleep(ref t, IntPtr.Zero);
            double n;
            for (var i = 0; i < 10; i++)
            {
                n = Stopwatch.GetTimestamp() / frequency * 1000d;
                nanosleep(ref t, IntPtr.Zero);
                n = Stopwatch.GetTimestamp() / frequency * 1000d - n;
                if (n > WaitResolution)
                    WaitResolution = n;
            }
            WaitResolution *= 1.25d;
        }

        /// <summary>
        /// Wait the specified number of milliseconds.
        /// </summary>
        public static void Wait(double milliseconds)
        {
            if (milliseconds < EPSILON)
                milliseconds = EPSILON;
            double start = Now;
            timespec t;
            t.tv_sec = IntPtr.Zero;
            t.tv_nsec = (IntPtr)500_000_000;
            while (milliseconds - Now + start > 1_000_000_000)
                nanosleep(ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)50_000_000;
            while (milliseconds - Now + start > 100_000_000)
                nanosleep(ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)5_000_000;
            while (milliseconds - Now + start > 10_000_000)
                nanosleep(ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)500_000;
            while (milliseconds - Now + start > 1_000_000)
                nanosleep(ref t, IntPtr.Zero);
            t.tv_nsec = (IntPtr)1_000;
            while (milliseconds - Now + start > WaitResolution)
                nanosleep(ref t, IntPtr.Zero);
            while (milliseconds - (Stopwatch.GetTimestamp() / frequency * 1000d) + start > 0)
            { }
        }

        /// <summary>
        /// Invoke an async callback once after n milliseconds.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait before the
        /// callback is invoked.</param>
        /// <param name="callback">The callback to invoke after n milliseconds.</param>
        public static async Task Once(double milliseconds, Action callback)
        {
            var timer = new PreciseTimer();
            await Task.Run(() =>
            {
                if (milliseconds < EPSILON)
                {
                    callback();
                    return;
                }
                Wait(milliseconds - timer.ElapsedMilliseconds);
                callback();
            });
        }

        /// <summary>
        /// Invoke an async callback every time n milliseconds have passed.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait before the
        /// callback is invoked.</param>
        /// <param name="callback">The callback to invoke every n milliseconds.
        /// When callback returns false the task is cancelled.</param>
        public static async Task Every(double milliseconds, Func<bool> callback)
        {
            var timer = new PreciseTimer();
            await Task.Run(() =>
            {
                if (milliseconds < EPSILON)
                    return;
                while (true)
                {
                    var w = timer.ElapsedMilliseconds % milliseconds;
                    Wait(milliseconds - w);
                    if (!callback())
                        break;
                }
            });
        }

        /// <summary>
        /// The minimum sleep time in milliseconds supported by your hardware.
        /// </summary>
        public static double WaitResolution { get; private set; }

        /// <summary>
        /// The absolute number of milliseconds passed since the start of 
        /// the epoch.
        /// </summary>
        public static double Now { get => Stopwatch.GetTimestamp() / frequency * 1000d; }

        public PreciseTimer()
        {
            Reset();
        }

        /// <summary>
        /// Reset the timer to zero.
        /// </summary>
        public void Reset()
        {
            if (enabled)
            {
                Enabled = false;
                Enabled = true;
            }
            else
                start = Stopwatch.GetTimestamp();
        }

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

        void ElapseTask(long id, double mark, double interval)
        {
            while (true)
            {
                var w = (ElapsedMilliseconds - mark) % interval;
                Wait(interval - w);
                if (id == taskId)
                    OnElapsed(this, EventArgs.Empty);
                else
                    break;

            }
        }

        /// <summary>
        /// The number of milliseconds to ellaspe before OnElapsed is invoked.
        /// </summary>
        public double Interval { get; set;  }

        Task task;
        bool enabled;
        long taskId;

        /// <summary>
        /// When enabled is set to true, OnElapsed will be invoked every interval
        /// number of milliseconds.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                var e = value && Interval > EPSILON;
                if (e == enabled)
                    return;
                enabled = e;
                taskId++;
                if (enabled)
                   task = Task.Run(() => ElapseTask(taskId, ElapsedMilliseconds, Interval));
                else
                {
                    task?.Wait();
                    task = null;
                }
            }
        }

        /// <summary>
        /// OnElapsed is invoked every interval number of milliseconds when enabled
        /// is set to true.
        /// </summary>
        public event EventHandler OnElapsed;

        /// <summary>
        /// Dispose releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Enabled = false;
        }
    }
}
