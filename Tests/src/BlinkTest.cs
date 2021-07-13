using System;
using System.Diagnostics;
using Codebot.Raspberry;

namespace Tests
{
    public static class BlinkTest
    {

        static double frequency = Stopwatch.Frequency;
        static Stopwatch watch;

        static void Wait(double time)
        {
            var start = Stopwatch.GetTimestamp();
            while ((Stopwatch.GetTimestamp() - start) / frequency * 1000d < time) ;
        }

        public static void Run()
        {
            var pinNumber = 4;
            Console.WriteLine($"LED Blink Test on GPIO {pinNumber}");
            var pin = Pi.Gpio.Pin(pinNumber);
            pin.Kind = PinKind.Output;
            pin.Write(false);
            watch = new Stopwatch();
            string s;
            double a, b;
            a = 1;
            while (true)
            {
                Console.WriteLine("What is the the blink interval in microseconds?");
                s = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(s))
                    b = a;
                else if (!double.TryParse(s, out b))
                    break;
                if (b <= 0)
                    b = a;
                a = b;
                Console.WriteLine($"Blinking at {a} interval.");
                pin.Write(true);
                Wait(a);
                pin.Write(false);
                Wait(a);

                pin.Write(true);
                Wait(a);
                pin.Write(false);
                Wait(a);

                pin.Write(true);
                Wait(a);
                pin.Write(false);
                Wait(a);

                pin.Write(true);
                Wait(a);
                pin.Write(false);
                Wait(a);

                pin.Write(true);
                Wait(a);
                pin.Write(false);
                Wait(a);
            }
        }
    }
}