using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT22
    /// </summary>
    public class Dht22 : Dhtxx
    {
        /// <summary>
        /// Create a DHT22 sensor
        /// </summary>
        public Dht22(int pin) : base(pin) { }

        protected override double GetHumidity(byte[] buffer)
        {
            return (buffer[0] << 8 | buffer[1]) * 0.1;
        }

        protected override Temperature GetTemperature(byte[] buffer)
        {
            var temp = ((buffer[2] & 0x7F) * 256 + buffer[3]) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((buffer[2] & 0x80) == 0 ? temp : -temp);
            return Temperature.FromCelsius(temp);
        }
    }
}
