using System;
using Codebot.Raspberry;

namespace Tests
{
    public static class SleepTest
    {
        static void TestPreciseWait()
        {
            Console.WriteLine("PreciseTimer wait resolution = {0:0.0000}",
                PreciseTimer.WaitResolution);
            PreciseTimer.Wait(100);
            double t;
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(100);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 100ms = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(10);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 10ms = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 1ms = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(0.5);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 500μs = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(0.1);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 100μs = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(0.01);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 10μs = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(0.001);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 1μs = {0:0.0000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.Now;
            PreciseTimer.Wait(0.0001);
            t = PreciseTimer.Now - t;
            Console.WriteLine("wait 100ns = {0:0.0000}", t);
        }


        public static void TimerElapsedTest()
        {
            var timer = new PreciseTimer();
            int i = 0;
            double a = 0;
            timer.OnElapsed += (sender, e) =>
            {
                i++;
                if (i == 1000)
                    a = PreciseTimer.Now;
                else if (i == 2000)
                    a = (PreciseTimer.Now - a) / 1000d;
            };
            i = 0;
            a = 0;
            timer.Interval = 1;
            timer.Enabled = true;
            PreciseTimer.Wait(5000);
            timer.Enabled = false;
            Console.WriteLine("expected interval = {0:0.000}, actual = {1:0.000}", timer.Interval, a);
            i = 0;
            a = 0;
            timer.Interval = 0.1;
            timer.Enabled = true;
            PreciseTimer.Wait(500);
            timer.Enabled = false;
            Console.WriteLine("expected interval = {0:0.000}, actual = {1:0.000}", timer.Interval, a);
            i = 0;
            a = 0;
            timer.Interval = 0.01;
            timer.Enabled = true;
            PreciseTimer.Wait(500);
            timer.Enabled = false;
            Console.WriteLine("expected interval = {0:0.000}, actual = {1:0.000}", timer.Interval, a);
            i = 0;
            a = 0;
            timer.Interval = 0.001;
            timer.Enabled = true;
            PreciseTimer.Wait(500);
            timer.Enabled = false;
            Console.WriteLine("expected interval = {0:0.000}, actual = {1:0.000}", timer.Interval, a);
        }

        public static void TimerEveryTest()
        {
            var a = new PreciseTimer();
            long i = 0;
            var running = true;
            var every = PreciseTimer.Every(1, () =>
            {
                i++;
                if (i % 1000 == 0)
                    Console.WriteLine("elapsed = {0:0.000}, i = {1}", a.ElapsedMilliseconds, i);
                return running;
            });
            Console.ReadLine();
            running = false;
            every.Wait();
        }

        public static void Run()
        {
            Console.WriteLine("nanosleep");
            for (var i = 0; i < 5; i++)
                TestPreciseWait();
        }
    }
}