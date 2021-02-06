using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device

{
    /// <summary>
    /// Temperature and Humidity Sensor DHT12
    /// </summary>
    public class Dht12 : Dhtxx
    {
        /// <summary>
        /// Create a DHT12 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        public Dht12(int pin) : base(pin) { }

        protected override double GetHumidity(byte[] buffer)
        {
            return buffer[0] + buffer[1] * 0.1;
        }

        protected override Temperature GetTemperature(byte[] buffer)
        {
            var temp = buffer[2] + (buffer[3] & 0x7F) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = (buffer[3] & 0x80) == 0 ? temp : -temp;
            return Temperature.FromCelsius(temp);
        }
    }
}
