using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class ServoTest
    {
        const int leftPin = 17;
        const int rightPin = 27;
        const int buttonPin = 22;
        const int maxAngle = 360;

        static void TestRotate()
        {
            Console.WriteLine("Testing a servo motor controlled by a rotary encoder");
            var angle = 0;
            const int step = 10;
            using (var left = Pi.Gpio.Pin(leftPin, PinKind.InputPullUp))
            using (var right = Pi.Gpio.Pin(rightPin, PinKind.InputPullUp))
            using (var servo = new ServoMotor(maxAngle))
            {
                servo.PulseWidths(0.5, 2.5);
                servo.Start();
                // Connect rotate and click events
                left.OnRisingEdge += Rotate;
                Console.WriteLine("Click the button to stop this test");
                Pi.Gpio.Pin(buttonPin, PinKind.InputPullUp).WaitForEdge(PinEdge.Rising);
                left.OnRisingEdge -= Rotate;
                servo.Angle = 0;
                Pi.Wait(2000);
                servo.Stop();

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
        }

        static void TestSweep()
        {
            Console.WriteLine("Testing a servo motor in a sweeping motion");
            var angle = 0d;
            var step = 0.1d;
            var done = false;
            using (var left = Pi.Gpio.Pin(leftPin, PinKind.InputPullUp))
            using (var right = Pi.Gpio.Pin(rightPin, PinKind.InputPullUp))
            using (var servo = new ServoMotor(maxAngle))
            {
                servo.PulseWidths(0.5, 2.5);
                servo.Start();
                var timer = PreciseTimer.Every(1, () =>
                {
                    if (done)
                        return false;
                    angle += step;
                    var a = angle % (maxAngle * 2);
                    if (a > maxAngle)
                        a = (maxAngle * 2) - a;
                    servo.Angle = a;
                    return true;
                });
                Console.WriteLine("Click the button to stop this test");
                // Connect rotate and click events
                left.OnRisingEdge += Rotate;
                Pi.Gpio.Pin(buttonPin, PinKind.InputPullUp).WaitForEdge(PinEdge.Rising);
                left.OnRisingEdge -= Rotate;
                done = true;
                timer.Wait();
                servo.Angle = 0;
                Pi.Wait(2000);

                // Called on the falling edge of the left pin
                void Rotate(object sender, PinEventHandlerArgs args)
                {
                    if (args.Bounced)
                        return;
                    var s = step;
                    if (left.Value == right.Value)
                        s += 0.1;
                    else
                        s -= 0.1;
                    if (s < 0)
                        s = 0;
                    step = s;
                    Console.WriteLine("servo step {0:0.0}°", step);
                }
            }
        }

        public static void Run()
        {
            TestRotate();
            TestSweep();
        }
    }
}
