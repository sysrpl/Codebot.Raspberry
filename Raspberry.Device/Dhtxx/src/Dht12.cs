// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Raspberry.Common;

namespace Raspberry.Device

{
    /// <summary>
    /// Temperature and Humidity Sensor DHT12
    /// </summary>
    public class Dht12 : Dhtxx
    {
        /// <summary>
        /// DHT12 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x5C;

        /// <summary>
        /// Create a DHT12 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        public Dht12(int pin) : base(pin) { }

        protected override double GetHumidity(byte[] readBuff)
        {
            return readBuff[0] + readBuff[1] * 0.1;
        }

        protected override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = readBuff[2] + (readBuff[3] & 0x7F) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = (readBuff[3] & 0x80) == 0 ? temp : -temp;

            return Temperature.FromCelsius(temp);
        }
    }
}
