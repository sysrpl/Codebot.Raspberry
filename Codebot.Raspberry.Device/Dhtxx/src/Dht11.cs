﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Raspberry.Common;

namespace Raspberry.Device
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT11
    /// </summary>
    public class Dht11 : Dhtxx
    {
        /// <summary>
        /// Create a DHT11 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        public Dht11(int pin) : base(pin) { }

        protected override double GetHumidity(byte[] readBuff)
        {
            return readBuff[0] + readBuff[1] * 0.1;
        }

        protected override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = readBuff[2] + readBuff[3] * 0.1;
            return Temperature.FromCelsius(temp);
        }
    }
}
