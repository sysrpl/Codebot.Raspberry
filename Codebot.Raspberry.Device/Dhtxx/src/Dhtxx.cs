// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// Temperature and Humidity Sensor DHTXX class of sensors
    /// </summary>
    [Device("DHTXX", "Temperature and Humidity Sensor", Category = "Sensor", Remarks = "Uses any GPIO")]
    public abstract class Dhtxx : HardwareDevice
    {
        protected byte[] buffer = new byte[5];
        protected readonly GpioPin pin;
        private double lastUpdate = 0;

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        protected Dhtxx(int pin)
        {
            this.pin = Pi.Gpio.Pin(pin);
            IsUpdateSuccessful = false;
            lastUpdate = Timer.Now;
        }

        /// <summary>
        /// IsLastReadSuccessful is true if the sensor was probed successfully
        /// </summary>
        public bool IsUpdateSuccessful { get; private set; }

        /// <summary>
        /// Update causes the sensor to be probed
        /// </summary>
        /// <returns>Returns true if the sensor probe was successful</returns>
        public override bool Update()
        {
            ReadData();
            return IsUpdateSuccessful;
        }

        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public virtual Temperature Temperature
        {
            get
            {
                return Update() ? GetTemperature(buffer) : Temperature.FromCelsius(double.NaN);
            }
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.NaN
        /// </remarks>
        public virtual double Humidity
        {
            get
            {
                return Update() ? GetHumidity(buffer) : double.NaN;
            }
        }

        /// <summary>
        /// Start reading data from the sensor
        /// </summary>
        protected virtual void ReadData()
        {
            if (Timer.Now - lastUpdate < 1)
                return;
            ReadPin();
        }

        /// <summary>
        /// Probe the sensor by reading pin data 
        /// </summary>
        private void ReadPin()
        {
            IsUpdateSuccessful = false;
            lastUpdate = Timer.Now;
            byte readVal = 0;
            // Keep data line HIGH
            pin.Mode = GpioPinMode.Output;
            pin.Write(true);
            Pi.Wait(20);
            // Send trigger signal
            pin.Write(false);
            // Wait at least 18 milliseconds or sensor initialization will fail
            Pi.Wait(20);
            pin.Write(true);
            // Wait 20 - 40 microseconds
            Pi.WaitMicroseconds(30);
            pin.Mode = GpioPinMode.InputPullUp;
            // LOW - about 80 microseconds
            var timer = new Timer();
            while (!pin.Read())
                if (timer.ElapsedMicroseconds > 100)
                {
                    IsUpdateSuccessful = false;
                    return;
                }
            // HIGH - about 80 microseconds
            timer.Reset();
            while (pin.Read())
            {
                if (timer.ElapsedMicroseconds > 100)
                {
                    IsUpdateSuccessful = false;
                    return;
                }
            }
            // The read data contains 40 bits
            for (int i = 0; i < 40; i++)
            {
                // Beginning signal per bit, about 50 microseconds
                timer.Reset();
                while (!pin.Read())
                {
                    if (timer.ElapsedMicroseconds > 100)
                    {
                        IsUpdateSuccessful = false;
                        return;
                    }
                }
                // 26 - 28 microseconds represent 0
                // 70 microseconds represent 1
                timer.Reset();
                while (pin.Read())
                {
                    if (timer.ElapsedMicroseconds > 100)
                    {
                        IsUpdateSuccessful = false;
                        return;
                    }
                }
                // bit to byte
                // less than 40 microseconds can be considered as 0, not necessarily less than 28 microseconds
                // here take 30 microseconds
                readVal <<= 1;
                if (!(timer.ElapsedMicroseconds <= 35))
                    readVal |= 1;
                if (((i + 1) % 8) == 0)
                    buffer[i / 8] = readVal;
            }
            if ((buffer[4] == ((buffer[0] + buffer[1] + buffer[2] + buffer[3]) & 0xFF)))
                IsUpdateSuccessful = (buffer[0] != 0) || (buffer[2] != 0);
            lastUpdate = Timer.Now;
        }

        /// <summary>
        /// Converting data to humidity
        /// </summary>
        /// <param name="readBuff">Data</param>
        /// <returns>Humidity</returns>
        protected abstract double GetHumidity(byte[] readBuff);

        /// <summary>
        /// Converting data to Temperature
        /// </summary>
        /// <param name="readBuff">Data</param>
        /// <returns>Temperature</returns>
        protected abstract Temperature GetTemperature(byte[] readBuff);
    }
}
