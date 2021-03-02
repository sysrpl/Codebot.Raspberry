using System;
using Codebot.Raspberry;
using static Codebot.Raspberry.Libc;

namespace Tests
{
    public static class SleepTest
    {
        static void TestPiWait()
        {
            Console.WriteLine("PreciseTimer wait resolution = {0:0.000}",
                PreciseTimer.WaitResolution);
            PreciseTimer.Wait(100);
            double t;
            PreciseTimer timer = new PreciseTimer();
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(100);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 100ms = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(10);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 10ms = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(1);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 1ms = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(0.5);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 500μs = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(0.1);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 100μs = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(0.01);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 10μs = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(0.001);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 1μs = {0:0.000}", t);
            Pi.Wait(1);
            timer.Reset();
            Pi.Wait(0.0001);
            t = timer.ElapsedMilliseconds;
            Console.WriteLine("wait 100ns = {0:0.000}", t);
        }

        static void TestPrecise()
        {
            Console.WriteLine("PreciseTimer wait resolution = {0:0.000}",
                PreciseTimer.WaitResolution);
            PreciseTimer.Wait(100);
            double t;
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(100);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 100ms = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(10);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 10ms = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 1ms = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(0.5);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 500μs = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(0.1);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 100μs = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(0.01);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 10μs = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(0.001);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 1μs = {0:0.000}", t);
            PreciseTimer.Wait(1);
            t = PreciseTimer.GetTime();
            PreciseTimer.Wait(0.0001);
            t = PreciseTimer.GetTime() - t;
            Console.WriteLine("wait 100ns = {0:0.000}", t);
        }

        public static void TimerElapsedTest()
        {
            var a = new PreciseTimer();
            a.Interval = 0.1;
            int i = 0;
            a.OnElapsed += (sender, e) =>
            {
                i++;
                if (i % 10000 == 0)
                    Console.WriteLine("elapsed = {0:0.000}, i = {1}", a.ElapsedMilliseconds, i);
            };
            a.Enabled = true;
            Console.ReadLine();
            a.Enabled = false;
            Console.ReadLine();
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
            var a = new PreciseTimer();
            var d = PreciseTimer.GetTime();
            while (a.ElapsedSeconds < 30)
            {
                var b = PreciseTimer.GetTime() - d;
                var c = a.ElapsedMilliseconds;
                Console.WriteLine("a = {0:0.000}, b = {1:0.000}, difference =  = {2:0.0000}", c, b, c - b);
                Pi.Wait(10);
            }

            //TimerEveryTest();
            /*for (var i =0; i < 5; i++)
            {
                Console.WriteLine($"nanosleep test #{i}");
                TestPrecise();
            }*/
        }
    }
}
