// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Raspberry.Common;

namespace Raspberry.Device
{ 
    /// <summary>
    /// Temperature and Humidity Sensor DHT10
    /// </summary>
    public class Dht10 : DhtBase
    {
        /// <summary>
        /// Create a DHT22 sensor
        /// </summary>
        public Dht10(int pin) : base(pin) { }

        // state, humi[20-13], humi[12-5], humi[4-1]temp[20-17], temp[16-9], temp[8-1]
        private readonly byte[] extraBuffer = new byte[6];

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public override double Humidity
        {
            get
            {
                ReadData();
                return GetHumidity(extraBuffer);
            }
        }

        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public override Temperature Temperature
        {
            get
            {
                ReadData();
                return GetTemperature(extraBuffer);
            }
        }

        protected override double GetHumidity(byte[] readBuff)
        {
            int raw = (((readBuff[1] << 8) | readBuff[2]) << 4) | readBuff[3] >> 4;
            return raw / Math.Pow(2, 20) * 100;
        }

        protected override Temperature GetTemperature(byte[] readBuff)
        {
            int raw = ((((readBuff[3] & 0b_0000_1111) << 8) | readBuff[4]) << 8) | readBuff[5];
            return Temperature.FromCelsius(raw / Math.Pow(2, 20) * 200 - 50);
        }
    }
}
