using System;
using Codebot.Raspberry;

namespace Tests
{
    public static class ButtonTest
    {
        public static void Run()
        {
            var ledNumber = 17;
            var buttonNumber = 27;
            Console.WriteLine($"LED Blink Test LED on GPIO {ledNumber} and button on GPIO {buttonNumber}");
            var led = Pi.Gpio.Pin(ledNumber);
            var button = Pi.Gpio.Pin(buttonNumber);
            led.Mode = GpioPinMode.Output;
            button.Mode = GpioPinMode.Input;
            int onMilliseconds = 100;
            int offMilliseconds = 100;
            try
            {
                while (true)
                {
                    if (button.Read())
                    {
                        Console.WriteLine($"Button is pressed");
                        Pi.Wait(onMilliseconds);
                        led.Write(true);
                        Pi.Wait(offMilliseconds);
                        led.Write(false);
                    }
                    else
                    {
                        Console.WriteLine($"Button is released");
                        Pi.Wait(onMilliseconds + offMilliseconds);
                        led.Write(false);
                    }
                }
            }
            finally
            {
                led.Write(false);
            }
        }
    }
}
