using System;
using Codebot.Raspberry;

namespace Tests
{
    public static class BlinkTest
    {
        public static void Run()
        {
            var pinNumber = 4;
            Console.WriteLine($"LED Blink Test on GPIO {pinNumber}");
            var pin = Pi.Gpio.Pin(pinNumber);
            pin.Kind = PinKind.Output;
            int onMilliseconds = 500;
            int offMilliseconds = 2000;
            try
            {
                while (true)
                {
                    Console.WriteLine($"Light is on for {onMilliseconds}ms");
                    Pi.Wait(onMilliseconds);
                    pin.Write(false);
                    Console.WriteLine($"Light is on for {offMilliseconds}ms");
                    Pi.Wait(offMilliseconds);
                }
            }
            finally
            {
                pin.WriteFast(false);
            }
        }
    }
}
