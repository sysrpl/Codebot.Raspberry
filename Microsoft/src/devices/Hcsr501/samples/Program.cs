﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.Gpio;

using Iot.Device.Hcsr501;

namespace Hcsr501.Samples
{
    internal class Program
    {
        private const int Hcsr501Pin = 17;
        private const int LedPin = 27;

        public static void Main(string[] args)
        {
            GpioController ledController = new GpioController();
            ledController.OpenPin(LedPin, PinMode.Output);

            using (Iot.Device.Hcsr501.Hcsr501 sensor =
                new Iot.Device.Hcsr501.Hcsr501(Hcsr501Pin))
            {
                while (true)
                {
                    // adjusting the detection distance and time by rotating the potentiometer on the sensor
                    if (sensor.IsMotionDetected)
                    {
                        // turn the led on when the sensor detected infrared heat
                        ledController.Write(LedPin, PinValue.High);
                        Console.WriteLine("Detected! Turn the LED on.");
                    }
                    else
                    {
                        // turn the led off when the sensor undetected infrared heat
                        ledController.Write(LedPin, PinValue.Low);
                        Console.WriteLine("Undetected! Turn the LED off.");
                    }

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
