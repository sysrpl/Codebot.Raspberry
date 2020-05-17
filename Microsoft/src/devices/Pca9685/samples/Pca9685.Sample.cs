﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Device.Pwm;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Iot.Device.ServoMotor;

namespace Iot.Device.Pwm.Pca9685Samples
{
    /// <summary>
    /// This sample is for Raspberry Pi Model 3B+
    /// </summary>
    public class Program
    {
        // Some SG90s can do 180 angle range but some other will be oscillating on the edge values
        // Max angle which doesn't cause any issues found experimentally was as below.
        // The ones which can do 180 will have the minimum pulse width at around 520uS.
        private const int AngleRange = 173;
        private const int MinPulseWidthMicroseconds = 600;
        private const int MaxPulseWidthMicroseconds = 2590;

        private static void PrintHelp()
        {
            Console.WriteLine("Command:");
            Console.WriteLine("    F {freq_hz}          set PWM frequency (Hz)");
            Console.WriteLine("    D {value}            set duty cycle (0.0 .. 1.0) for all channels");
            Console.WriteLine("    S {channel} {value}  set duty cycle (0.0 .. 1.0) for specific channel");
            Console.WriteLine("    T {channel}          servo test (defaults to SG90 params)");
            Console.WriteLine("    H                    prints help");
            Console.WriteLine("    Q                    quit");
            Console.WriteLine();
        }

        /// <summary>
        /// Example program main entry point
        /// </summary>
        /// <param name="args">Command line arguments, see <see cref="PrintHelp"/></param>
        public static void Main(string[] args)
        {
            var busId = 1;
            var selectedI2cAddress = 0b000000; // A5 A4 A3 A2 A1 A0
            var deviceAddress = Pca9685.I2cAddressBase + selectedI2cAddress;

            var settings = new I2cConnectionSettings(busId, deviceAddress);
            var device = I2cDevice.Create(settings);

            using (var pca9685 = new Pca9685(device))
            {
                Console.WriteLine(
                    $"PCA9685 is ready on I2C bus {device.ConnectionSettings.BusId} with address {device.ConnectionSettings.DeviceAddress}");
                Console.WriteLine($"PWM Frequency: {pca9685.PwmFrequency}Hz");
                Console.WriteLine();
                PrintHelp();

                while (true)
                {
                    var command = Console.ReadLine().ToLower().Split(' ');
                    if (string.IsNullOrEmpty(command[0]))
                    {
                        return;
                    }

                    switch (command[0][0])
                    {
                        case 'q':
                            pca9685.SetDutyCycleAllChannels(0.0);
                            return;
                        case 'f':
                        {
                            var freq = double.Parse(command[1]);
                            pca9685.PwmFrequency = freq;
                            Console.WriteLine($"PWM Frequency has been set to {pca9685.PwmFrequency}Hz");
                            break;
                        }

                        case 'd':
                        {
                            switch (command.Length)
                            {
                                case 2:
                                {
                                    double value = double.Parse(command[1]);
                                    pca9685.SetDutyCycleAllChannels(value);
                                    Console.WriteLine($"PWM duty cycle has been set to {value}");
                                    break;
                                }

                                case 3:
                                {
                                    int channel = int.Parse(command[1]);
                                    double value = double.Parse(command[2]);
                                    pca9685.SetDutyCycle(channel, value);
                                    Console.WriteLine($"PWM duty cycle has been set to {value}");
                                    break;
                                }
                            }

                            break;
                        }

                        case 'h':
                        {
                            PrintHelp();
                            break;
                        }

                        case 't':
                        {
                            int channel = int.Parse(command[1]);
                            ServoDemo(pca9685, channel);
                            PrintHelp();
                            break;
                        }
                    }
                }
            }
        }

        private static ServoMotor.ServoMotor CreateServo(Pca9685 pca9685, int channel)
        {
            return new ServoMotor.ServoMotor(
                pca9685.CreatePwmChannel(channel),
                AngleRange, MinPulseWidthMicroseconds, MaxPulseWidthMicroseconds);
        }

        private static void PrintServoDemoHelp()
        {
            Console.WriteLine("Q                   return to previous menu");
            Console.WriteLine("C                   calibrate");
            Console.WriteLine("{angle}             set angle (0 - 180)");
            Console.WriteLine();
        }

        private static void ServoDemo(Pca9685 pca9685, int channel)
        {
            using (var servo = CreateServo(pca9685, channel))
            {
                PrintServoDemoHelp();

                while (true)
                {
                    var command = Console.ReadLine().ToLower();
                    if (string.IsNullOrEmpty(command) || command[0] == 'q')
                    {
                        return;
                    }

                    if (command[0] == 'c')
                    {
                        CalibrateServo(servo);
                        PrintServoDemoHelp();
                    }
                    else
                    {
                        double value = double.Parse(command);
                        servo.WriteAngle(value);
                        Console.WriteLine($"Angle set to {value}. PWM duty cycle = {pca9685.GetDutyCycle(channel)}");
                    }
                }
            }
        }

        private static void CalibrateServo(ServoMotor.ServoMotor servo)
        {
            int maximumAngle = 180;
            int minimumPulseWidthMicroseconds = 520;
            int maximumPulseWidthMicroseconds = 2590;

            Console.WriteLine("Searching for minimum pulse width");
            CalibratePulseWidth(servo, ref minimumPulseWidthMicroseconds);
            Console.WriteLine();

            Console.WriteLine("Searching for maximum pulse width");
            CalibratePulseWidth(servo, ref maximumPulseWidthMicroseconds);

            Console.WriteLine("Searching for angle range");
            Console.WriteLine(
                "What is the angle range? (type integer with your angle range or enter to move to MIN/MAX)");

            while (true)
            {
                servo.WritePulseWidth(maximumPulseWidthMicroseconds);
                Console.WriteLine("Servo is now at MAX");
                if (int.TryParse(Console.ReadLine(), out maximumAngle))
                {
                    break;
                }

                servo.WritePulseWidth(minimumPulseWidthMicroseconds);
                Console.WriteLine("Servo is now at MIN");

                if (int.TryParse(Console.ReadLine(), out maximumAngle))
                {
                    break;
                }
            }

            servo.Calibrate(maximumAngle, minimumPulseWidthMicroseconds, maximumPulseWidthMicroseconds);
            Console.WriteLine($"Angle range: {maximumAngle}");
            Console.WriteLine($"Min PW [uS]: {minimumPulseWidthMicroseconds}");
            Console.WriteLine($"Max PW [uS]: {maximumPulseWidthMicroseconds}");
        }

        private static void CalibratePulseWidth(ServoMotor.ServoMotor servo, ref int pulseWidthMicroSeconds)
        {
            void SetPulseWidth(ref int pulseWidth)
            {
                pulseWidth = Math.Max(pulseWidth, 0);
                servo.WritePulseWidth(pulseWidth);
            }

            Console.WriteLine("Use A/Z (1x); S/X (10x); D/C (100x)");
            Console.WriteLine("Press enter to accept value");

            while (true)
            {
                SetPulseWidth(ref pulseWidthMicroSeconds);
                Console.WriteLine($"Current value: {pulseWidthMicroSeconds}");

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.A:
                        pulseWidthMicroSeconds++;
                        break;
                    case ConsoleKey.Z:
                        pulseWidthMicroSeconds--;
                        break;
                    case ConsoleKey.S:
                        pulseWidthMicroSeconds += 10;
                        break;
                    case ConsoleKey.X:
                        pulseWidthMicroSeconds -= 10;
                        break;
                    case ConsoleKey.D:
                        pulseWidthMicroSeconds += 100;
                        break;
                    case ConsoleKey.C:
                        pulseWidthMicroSeconds -= 100;
                        break;
                    case ConsoleKey.Enter:
                        return;
                }
            }
        }
    }
}
