﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// To support the grove Temperature sensor http://wiki.seeed.cc/Grove-Temperature_Sensor_V1.2
    /// </summary>
    public class GroveTemperatureSensor : AnalogSensor
    {
        /// <summary>
        /// Grove temperature sensor constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public GroveTemperatureSensor(GrovePi grovePi, GrovePort port)
            : base(grovePi, port)
        {
        }

        /// <summary>
        /// Get the temperature in Celsius
        /// </summary>
        public double Temperature
        {
            get
            {
                var ret = Value;
                return 1 / Math.Log(((MaxAdc - ret) * 1000 / ret) / 4275 + 1 / 296.15) - 273.15;
            }
        }

        /// <summary>
        /// Get the temperature in Farenheit
        /// </summary>
        public double TemperatureInFarenheit => Temperature * 9 / 5 + 32;

        /// <summary>
        /// Returns the temperature formated in Celsius.
        /// </summary>
        /// <returns>Returns the temperature formated in Celsius</returns>
        public override string ToString() => $"{Temperature} °C";

        /// <summary>
        /// Get the name Grove Temperature Sensor
        /// </summary>
        public new string SensorName => "Grove Temperature Sensor";

    }
}
