using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
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

        protected override double GetHumidity(byte[] buffer)
        {
            return buffer[0] + buffer[1] * 0.1;
        }

        protected override Temperature GetTemperature(byte[] buffer)
        {
            var temp = buffer[2] + buffer[3] * 0.1;
            return Temperature.FromCelsius(temp);
        }
    }
}
