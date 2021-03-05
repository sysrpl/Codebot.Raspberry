using System;
using Codebot.Raspberry;

namespace Tests
{
    public static class ButtonTest
    {
        static void TestPolling()
        {
            var ledNumber = 27;
            var buttonNumber = 17;
            Console.WriteLine($"LED Blink Test LED on GPIO {ledNumber} and button on GPIO {buttonNumber}");
            var led = Pi.Gpio.Pin(ledNumber);
            var button = Pi.Gpio.Pin(buttonNumber);
            led.Kind = PinKind.Output;
            button.Kind = PinKind.InputPullUp;
            int onMilliseconds = 100;
            int offMilliseconds = 100;
            try
            {
                while (true)
                {
                    if (!button.Read())
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

        static void TesButtonEvent()
        {
            var buttonNumber = 17;
            var i = 0;
            Console.WriteLine($"Button event test on GPIO {buttonNumber}");
            var button = Pi.Gpio.Pin(buttonNumber);
            button.Kind = PinKind.InputPullUp;
            button.OnRisingEdge += Click;
            while (i < 10)
                Pi.Wait(100);
            button.OnRisingEdge -= Click;

            void Click(object sender, PinEventHandlerArgs e)
            {
                if (e.Bounced)
                    return;
                Console.WriteLine($"Click {i++}");
            }
        }

        public static void Run()
        {
            TesButtonEvent();
        }
    }
}
