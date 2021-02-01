using System;
using Codebot.Raspberry;

namespace Tests
{
    public static class BlinkTest
    {
        public static void Run()
        {
            var pinNumber = 17;
            Console.WriteLine($"LED Blink Test on GPIO {pinNumber}");
            var pin = Pi.Gpio.Pin(pinNumber);
            pin.Mode = GpioPinMode.Output;
            int onMilliseconds = 50;
            int offMilliseconds = 50;
            try
            {
                while (true)
                {
                    Console.WriteLine($"Light is on for {onMilliseconds}ms");
                    Pi.Wait(onMilliseconds);
                    pin.Write(true);
                    Console.WriteLine($"Light is off for {offMilliseconds}ms");
                    Pi.Wait(offMilliseconds);
                    pin.Write(false);
                }
            }
            finally
            {
                pin.Write(false);
            }
        }
    }
}
