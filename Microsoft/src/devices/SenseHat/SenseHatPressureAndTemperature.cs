﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT - Pressure and temperature sensor
    /// </summary>
    public class SenseHatPressureAndTemperature : Lps25h.Lps25h
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const int I2cAddress = 0x5c;

        /// <summary>
        /// Constructs SenseHatPressureAndTemperature instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        public SenseHatPressureAndTemperature(I2cDevice i2cDevice = null)
            : base(i2cDevice ?? CreateDefaultI2cDevice())
        {
        }

        private static I2cDevice CreateDefaultI2cDevice()
        {
            var settings = new I2cConnectionSettings(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
