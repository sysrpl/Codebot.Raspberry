using System;
using Codebot.Raspberry;
using static System.Console;

namespace Tests
{
    public static class PulseTest
    {
        static void RunEncoder()
        {
            WriteLine($"Pulse Width Modulation Test on GPIO 18");
            WriteLine($"Using rotary encoder");
            var running = true;
            var pwm = new PwmChannel(0, 0);
            var period = 0.1d;
            WriteLine($"Running PWM with period of {period}ms.");
            pwm.Period = (ulong)(period * 1_000_000d);
            pwm.DutyCycle = 0.5d;
            pwm.Enable();

            var button = Pi.Gpio.Pin(25, PinKind.InputPullUp);
            button.OnRisingEdge += (sender, args) =>
            {
                if (args.Bounced)
                    return;
                running = false;
            };

            var left = Pi.Gpio.Pin(23, PinKind.InputPullUp);
            var right = Pi.Gpio.Pin(24, PinKind.InputPullUp);

            left.OnRisingEdge += (sender, args) =>
            {
                if (args.Bounced)
                    return;
                if (left.Value == right.Value)
                    period *= 0.9d;
                else
                    period *= 1d / 0.9d;
                pwm.Period = (ulong)(period * 1_000_000d);
                WriteLine($"Changed PWM period to {period}ms.");
            };

            try
            {
                while (running) 
                    Pi.Wait(50);
            }
            finally
            {
                button.RemoveEvents();
                button.Dispose();
                left.RemoveEvents();
                left.Dispose();
                pwm.Disable();
                pwm.Dispose();
            }
        }

        static void RunKeyboard()
        {
            WriteLine($"Pulse Width Modulation Test on GPIO 18");
            WriteLine($"Using keyboard input parameters");
            string s;
            double p = 0.1;
            double d = 0.5;
            using (var pwm = new PwmChannel(0, 0))
            {
                while (true)
                {
                    WriteLine($"Running PWM period {p}ms and duty cycle {d}.");
                    pwm.Disable();
                    pwm.Period = (ulong)(p * 1_000_000d);
                    pwm.DutyCycle = d;
                    Pi.Wait(1);
                    pwm.Enable();
                    WriteLine($"Enter pulse period in milliseconds ({p}ms):");
                    s = ReadLine();
                    if (string.IsNullOrWhiteSpace(s))
                        s = p.ToString();
                    if (!double.TryParse(s, out p) || p <= 0)
                        break;
                    WriteLine($"Enter duty cycle in percent ({d}):");
                    s = ReadLine();
                    if (string.IsNullOrWhiteSpace(s))
                        s = d.ToString();
                    if (!double.TryParse(s, out d) || d <= 0 || d >= 1)
                        break;
                }
                pwm.Disable();
            }
        }

        public static void Run()
        {
            //RunKeyboard();
            RunEncoder();
        }
    }
}
