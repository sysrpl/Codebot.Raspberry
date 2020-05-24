﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT21
    /// </summary>
    public class Dht21 : Dhtxx
    {
        /// <summary>
        /// Create a DHT22 sensor
        /// </summary>
        public Dht21(int pin) : base(pin) { }

        protected override double GetHumidity(byte[] readBuff)
        {
            return (readBuff[0] << 8 | readBuff[1]) * 0.1;
        }

        protected override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = (readBuff[2] & 0x7F) + readBuff[3] * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((readBuff[2] & 0x80) == 0 ? temp : -temp);
            return Temperature.FromCelsius(temp);
        }
    }
}
