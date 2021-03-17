using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class StepperTest
    {
        static StepperMotor MotorCreate()
        {
            // var driver = new UnipoleDriver(22, 27, 17, 4);
            var driver = new BipoleDriver(1.8d, 13, 19, 6, 16, 20, 21);
            return new StepperMotor(driver)
            {
                Mode = BipoleMode.SixteenthStep
            };
        }

        static void MoveTest()
        {
            Console.WriteLine("Stepper motor rotate by input test half step");
            using (var motor = MotorCreate())
            {
                motor.RPM = 15;
                while (true)
                {
                    Console.WriteLine($"Rotate motor by how many degrees?");
                    if (double.TryParse(Console.ReadLine(), out double a))
                    {
                        motor.Angle += a;
                        motor.Wait();
                    }
                    else
                        break;
                }
            }
        }

        static void MotorThenAngleTest()
        {
            const double delay = 2000;

            Console.WriteLine("Stepper motor then angle test with driver half step");
            using (var motor = MotorCreate())
            {
                motor.OnStepTaskComplete += TaskComplete;
                int i = 0;
                while (i++ < 1000)
                {
                    Console.WriteLine($"Then angle test itteration {i}");
                    motor
                        .MoveAngle(0)
                        .ThenAngle(30, StepperMove.Absolute, delay, 100)
                        .ThenAngle(90, StepperMove.Absolute, delay, 100)
                        .ThenAngle(-90, StepperMove.Absolute, delay, 200)
                        .ThenAngle(180, StepperMove.Absolute, delay, 150);
                    for (var j = 0; j < 9; j++)
                        motor.ThenAngle(10, StepperMove.Relative, delay, 125);
                    motor.Wait(delay);
                }
                motor.MoveAngle(0).Wait();
                motor.OnStepTaskComplete -= TaskComplete;

                void TaskComplete(object sender, EventArgs e)
                {
                    Console.WriteLine($"motor at angle {motor.Angle}");
                }
            }
        }

        static void SpeedTest()
        {
            Console.WriteLine("Stepper motor RPM test");
            using (var motor = MotorCreate())
            {
                motor.RPM = 29;
                double a, b;
                int i = 0;
                int j = 0;
                while (motor.RPM < 200)
                {
                    Console.WriteLine($"");
                    Console.WriteLine($"rotation {++i}, motor at RPM {++motor.RPM}");
                    a = PreciseTimer.Now;
                    motor.MoveAngle(360 * ++j).Wait();
                    a = PreciseTimer.Now - a;
                    b = 60d / motor.RPM * 1000d;
                    Console.WriteLine($"actual time {a:0.000}ms, expected time {b:0.000}ms, difference {b - a:0.000}");
                    a = a / motor.SPR;
                    b = b / motor.SPR;
                    Console.WriteLine($"actual step time {a:0.000}ms, expected step time {b:0.000}ms");
                    Pi.Wait(1000);
                }
            }
        }

        public static void Run()
        {
            MoveTest();
            MotorThenAngleTest();
            // SpeedTest();
        }
    }
}
