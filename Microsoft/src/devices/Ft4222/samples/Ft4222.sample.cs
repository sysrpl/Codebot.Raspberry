﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Bno055;
using Iot.Device.Ft4222;

namespace Ft4222.Samples
{
    /// <summary>
    /// Sample application for Ft4222
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Unused</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello I2C, SPI and GPIO FTFI! FT4222");
            Console.WriteLine("Select the test you want to run:");
            Console.WriteLine(" 1 Run I2C tests with a BNO055");
            Console.WriteLine(" 2 Run SPI tests with a simple HC595 with led blinking on all ports");
            Console.WriteLine(" 3 Run GPIO tests with a simple led blinking on GPIO2 port and reading the port");
            Console.WriteLine(" 4 Run callback test event on GPIO2 on Failing and Rising");
            var key = Console.ReadKey();
            Console.WriteLine();

            var devices = FtCommon.GetDevices();
            Console.WriteLine($"{devices.Count} FT4222 elements found");
            foreach (var device in devices)
            {
                Console.WriteLine($"Description: {device.Description}");
                Console.WriteLine($"Flags: {device.Flags}");
                Console.WriteLine($"Id: {device.Id}");
                Console.WriteLine($"Location Id: {device.LocId}");
                Console.WriteLine($"Serial Number: {device.SerialNumber}");
                Console.WriteLine($"Device type: {device.Type}");
            }

            var (chip, dll) = FtCommon.GetVersions();
            Console.WriteLine($"Chip version: {chip}");
            Console.WriteLine($"Dll version: {dll}");

            if (key.KeyChar == '1')
            {
                TestI2c();
            }

            if (key.KeyChar == '2')
            {
                TestSpi();
            }

            if (key.KeyChar == '3')
            {
                TestGpio();
            }

            if (key.KeyChar == '4')
            {
                TestEvents();
            }
        }

        private static void TestI2c()
        {
            var ftI2c = new Ft4222I2c(new I2cConnectionSettings(0, Bno055Sensor.DefaultI2cAddress));

            var bno055Sensor = new Bno055Sensor(ftI2c);

            Console.WriteLine($"Id: {bno055Sensor.Info.ChipId}, AccId: {bno055Sensor.Info.AcceleratorId}, GyroId: {bno055Sensor.Info.GyroscopeId}, MagId: {bno055Sensor.Info.MagnetometerId}");
            Console.WriteLine($"Firmware version: {bno055Sensor.Info.FirmwareVersion}, Bootloader: {bno055Sensor.Info.BootloaderVersion}");
            Console.WriteLine($"Temperature source: {bno055Sensor.TemperatureSource}, Operation mode: {bno055Sensor.OperationMode}, Units: {bno055Sensor.Units}");
            Console.WriteLine($"Powermode: {bno055Sensor.PowerMode}");
        }

        private static void TestSpi()
        {
            var ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = 1_000_000, Mode = SpiMode.Mode0 });

            while (!Console.KeyAvailable)
            {
                ftSpi.WriteByte(0xFF);
                Thread.Sleep(500);
                ftSpi.WriteByte(0x00);
                Thread.Sleep(500);
            }
        }

        public static void TestGpio()
        {
            const int Gpio2 = 2;
            var gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());

            // Opening GPIO2
            gpioController.OpenPin(Gpio2);
            gpioController.SetPinMode(Gpio2, PinMode.Output);

            Console.WriteLine("Blinking GPIO2");
            while (!Console.KeyAvailable)
            {
                gpioController.Write(Gpio2, PinValue.High);
                Thread.Sleep(500);
                gpioController.Write(Gpio2, PinValue.Low);
                Thread.Sleep(500);
            }

            Console.ReadKey();
            Console.WriteLine("Reading GPIO2 state");
            gpioController.SetPinMode(Gpio2, PinMode.Input);
            while (!Console.KeyAvailable)
            {
                Console.Write($"State: {gpioController.Read(Gpio2)} ");
                Console.CursorLeft = 0;
                Thread.Sleep(50);
            }
        }

        public static void TestEvents()
        {
            const int Gpio2 = 2;
            var gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());

            // Opening GPIO2
            gpioController.OpenPin(Gpio2);
            gpioController.SetPinMode(Gpio2, PinMode.Input);

            Console.WriteLine("Setting up events on GPIO2 for rising and failing");

            gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, MyCallbackFailing);

            Console.WriteLine("Event setup, press a key to remove the failing event");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Falling, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    MyCallbackFailing(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
                }

                res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    MyCallbackFailing(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
                }
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallbackFailing);
            gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Rising, MyCallback);

            Console.WriteLine("Event removed, press a key to remove all events and quit");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    MyCallback(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
                }
            }

            gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
        }

        private static void MyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");
        }

        private static void MyCallbackFailing(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");
        }
    }
}
