using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// Temperature and Humidity Sensor DHTXX class of sensors
    /// </summary>
    [Device("DHTXX", "Temperature and Humidity Sensor", Category = "Sensor", Remarks = "Uses any GPIO")]
    public abstract class Dhtxx : HardwareDevice
    {
        protected readonly byte[] buffer;
        private readonly GpioPin pin;
        private double lastUpdate;

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pin">The logical (GPIO) pin number</param>
        protected Dhtxx(int pin)
        {
            this.buffer = new byte[5];
            this.pin = Pi.Gpio.Pin(pin);
            // Set pin to high for next update
            this.pin.Kind = PinKind.Output;
            this.pin.Write(true);
            IsUpdateSuccessful = false;
            lastUpdate = Pi.Now;
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
            if (Pi.Now - lastUpdate < 1)
                return;
            ReadPin();
        }

        /// <summary>
        /// Probe the sensor by reading pin data 
        /// </summary>
        private void ReadPin()
        {
            IsUpdateSuccessful = false;
            try
            {
                // Send the sensor our trigger signal
                pin.Write(false);
                // Wait at least 18ms for sensor initialization
                Pi.Wait(20);
                // Send the start signal
                pin.Write(true);
                // Wait 20μs - 40μs
                Pi.WaitMicroseconds(30);
                pin.Kind = PinKind.InputPullUp;
                // Sensor should take over and go low
                if (!pin.WaitLow(Pi.Microseconds(100)))
                    // FAIL: sensor did not take over
                    return;
                // Wait for the sensor to switch to high
                if (!pin.WaitHigh(Pi.Microseconds(100)))
                    // FAIL: sensor did not send high
                    return;
                // Wait for sensor to begin data bits
                if (!pin.WaitLow(Pi.Microseconds(100)))
                    // FAIL: sensor did not begin sending a data bit
                    return;
                // Elapsed is the duration a data bit was held high 
                double elapsed = 0;
                // A buffer for eight bits of data
                byte data = 0;
                // Now we can begin reading 40 data bits
                for (int i = 0; i < 40; i++)
                {
                    // Wait for the data bit
                    if (!pin.WaitHigh(Pi.Microseconds(100)))
                        // FAIL: no data bit was sent
                        return;
                    // Measure the data bit
                    if (!pin.WaitLow(Pi.Microseconds(100), out elapsed))
                        // FAIL: data bit was too long
                        return;
                    // Make room to store the next data bit
                    data <<= 1;
                    // If elapsed was more than 40µs then the data bit is 1
                    if (elapsed > 0.04d)
                        data |= 1;
                    // If 8 bits were read then copy those bits to our buffer
                    if (((i + 1) % 8) == 0)
                        buffer[i / 8] = data;
                }
                // Success is reached if the checksum passes
                if ((buffer[4] == ((buffer[0] + buffer[1] + buffer[2] + buffer[3]) & 0xFF)))
                    IsUpdateSuccessful = (buffer[0] != 0) || (buffer[2] != 0);
            }
            finally
            {
                // Set pin to high and for next update
                pin.Kind = PinKind.Output;
                pin.Write(true);
                // And set the last update time
                lastUpdate = Pi.Now;
            }
        }

        /// <summary>
        /// Converting data to humidity
        /// </summary>
        /// <param name="buffer">Data</param>
        /// <returns>Humidity</returns>
        protected abstract double GetHumidity(byte[] buffer);

        /// <summary>
        /// Converting data to Temperature
        /// </summary>
        /// <param name="buffer">Data</param>
        /// <returns>Temperature</returns>
        protected abstract Temperature GetTemperature(byte[] buffer);
    }
}
