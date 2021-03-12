using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class StepperTest
    {

        static Uln2003 MotorCreate()
        {
            return new Uln2003(22, 27, 17, 4);
        }

        static void MoveTest()
        {
            Console.WriteLine("Stepper motor rotate by input test");
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

            Console.WriteLine("Stepper motor then angle test");
            using (var motor = MotorCreate())
            {
                motor.OnStepTaskComplete += TaskComplete;
                int i = 0;
                while (i++ < 1000)
                {
                    Console.WriteLine($"Then angle test itteration {i}");
                    motor
                        .MoveAngle(0)
                        .ThenAngle(30, StepperMove.Absolute, delay, 2)
                        .ThenAngle(90, StepperMove.Absolute, delay, 2)
                        .ThenAngle(-90, StepperMove.Absolute, delay, 10)
                        .ThenAngle(180, StepperMove.Absolute, delay, 15);
                    for (var j = 0; j < 9; j++)
                        motor.ThenAngle(10, StepperMove.Relative, delay, 12.5);
                    motor.Wait(delay);
                }
                motor.MoveAngle(0).Wait();

                void TaskComplete(object sender, EventArgs e)
                {
                    Console.WriteLine($"motor at angle {motor.Angle}");
                }
            }
        }

        static void RepeatTest()
        {
            Console.WriteLine("Stepper motor RPM test");
            using (var motor = MotorCreate())
            {
                motor.RPM = 15;
                int i = 0;
                while (i < 5)
                {
                    Console.WriteLine($"rotation {i++}, motor at {motor.Position / -4096d:0.0000}");
                    motor.MoveAngle(360);
                    motor.Wait();
                    Pi.Wait(3000);
                }
            }
        }

        static void SpeedTest()
        {
            Console.WriteLine("Stepper motor repeatability test");
            using (var motor = MotorCreate())
            {
                var running = true;

                var task = PreciseTimer.Every(1000, () =>
                {
                    Console.WriteLine("motor angle = {0:0.0}°", motor.Angle % 360d);
                    return running;
                });

                int i = 10;
                while (running)
                {
                    motor.RPM = i++;
                    Console.WriteLine("{0} RPM", motor.RPM);
                    motor.Angle += 180;
                    motor.Wait();
                    running = i < 21;
                }

                task.Wait();
            }
        }

        public static void Run()
        {
            MoveTest();
            MotorThenAngleTest();
        }
    }
}
