using System;
using Codebot.Raspberry;
using static System.Console;

namespace Tests
{
    public static class TimingTest
    {
        public static void Run()
        {
            var pinNumber = 4;
            Console.WriteLine($"Timing Test on GPIO {pinNumber}");
            var pin = Pi.Gpio.Pin(pinNumber);
            pin.Kind = PinKind.Output;
            Pi.Wait(0.5);
            string s;
            double w = 1;
            while (true)
            {
                WriteLine("Enter timing interval in milliseconds:");
                s = ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                    s = w.ToString();
                if (!double.TryParse(s, out w) || w <= 0)
                    return;
                WriteLine($"Generating pulses at an interval of {w}ms.");
                Pi.Wait(w);
                pin.Write(true);
                Pi.Wait(w);
                pin.Write(false);
                Pi.Wait(w);
                pin.Write(true);
                Pi.Wait(w);
                pin.Write(false);
                Pi.Wait(w);
                pin.Write(true);
                Pi.Wait(w);
                pin.Write(false);
                Pi.Wait(w);
                pin.Write(true);
                Pi.Wait(w);
                pin.Write(false);
                Pi.Wait(w);
                pin.Write(true);
                Pi.Wait(w);
                pin.Write(false);
            }
        }
    }
}
