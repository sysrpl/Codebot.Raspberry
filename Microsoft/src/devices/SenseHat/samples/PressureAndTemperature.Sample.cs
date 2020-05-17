﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Iot.Device.Common;
using Iot.Units;

namespace Iot.Device.SenseHat.Samples
{
    internal class PressureAndTemperature
    {
        public static void Run()
        {
            // set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = Pressure.MeanSeaLevel;

            using (var pt = new SenseHatPressureAndTemperature())
            {
                while (true)
                {
                    var tempValue = pt.Temperature;
                    var preValue = pt.Pressure;
                    var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

                    Console.WriteLine($"Temperature: {tempValue.Celsius:0.#}\u00B0C");
                    Console.WriteLine($"Pressure: {preValue.Hectopascal:0.##}hPa");
                    Console.WriteLine($"Altitude: {altValue:0.##}m");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
