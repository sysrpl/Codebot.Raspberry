using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class ServoTest
    {
        const int leftPin = 17;
        const int rightPin = 27;
        const int step = 10;
        const int maxAngle = 360;

        static void Test()
        {
            Console.WriteLine("Testing a servo motor controlled by a rotary encoder");
            var angle = 0;
            var left = Pi.Gpio.Pin(leftPin, PinKind.InputPullUp);
            var right = Pi.Gpio.Pin(rightPin, PinKind.InputPullUp);
            var servo = new ServoMotor(maxAngle);
            servo.PulseWidths(0.5, 2.5);
            servo.Start();
            // Connect rotate and click events
            left.OnRisingEdge += Rotate;
            Console.WriteLine("Press enter to STOP");
            Console.ReadLine();
            left.OnRisingEdge -= Rotate;
            servo.Stop();
            servo.Dispose();
            Pi.Gpio.Close();

            // Called on the falling edge of the left pin
            void Rotate(object sender, PinEventHandlerArgs args)
            {
                if (args.Bounced)
                    return;
                var a = angle;
                a += left.Value == right.Value ? -step : step;
                if (a < 0)
                    a = 0;
                if (a > maxAngle)
                    a = maxAngle;
                if (a != angle)
                    Console.WriteLine("servo angle at {0}°", servo.Angle = angle = a);
            }
        }

        public static void Run()
        {
            Test();
        }
    }
}
