using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class ServoTest
    {
        const int leftPin = 17;
        const int rightPin = 27;

        static void Test()
        {
            Console.WriteLine("Testing a servo motor controlled by a rotary encoder");
            var position = 1;
            var left = Pi.Gpio.Pin(leftPin, PinKind.InputPullUp);
            var right = Pi.Gpio.Pin(rightPin, PinKind.InputPullUp);
            var servo = new ServoMotor();
            servo.Start();
            // Connect rotate and click events
            left.OnRisingEdge += Rotate;
            Console.WriteLine("Press enter to STOP");
            Console.ReadLine();
            left.OnRisingEdge -= Rotate;
            servo.Stop();
            servo.Dispose();

            // Called on the falling edge of the left pin
            void Rotate(object sender, PinEventHandlerArgs args)
            {
                if (args.Bounced)
                    return;
                if (left.Value == right.Value)
                {
                    Console.WriteLine("counterclockwise rotate");
                    position--;
                }
                else
                {
                    Console.WriteLine("clockwise rotate");
                    position++;
                }
                Console.WriteLine("rotate position {0}", position);
                servo.Angle = position * 10;
            }
        }

        public static void Run()
        {
            Test();
        }
    }
}
