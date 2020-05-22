using System.Drawing;
using Raspberry.Board.Spi;

namespace Raspberry.Device
{
    /// <summary>
    /// The Ws28xx class represents a strand of neopixels. This class uses
    /// SPI to communicate efficiently and assumes use of pin GPIO 10
    /// </summary>
    public class Ws28xx
    {
        SpiDevice device;
        Pixels pixels;

        /// <summary>
        /// Create a new device with count number of pixels
        /// </summary>
        public Ws28xx(int count)
        {
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 2_400_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            device = SpiDevice.Create(settings);
            pixels = new Pixels(count);
        }

        /// <summary>
        /// The number of pixels in this device
        /// </summary>
        public int Count
        {
            get => pixels.Count;
            set => pixels.Count = value;
        }

        /// <summary>
        /// Set the clor of a pixel at the specified index
        /// </summary>
        public void SetPixel(int index, Color color) => pixels.SetPixel(index, color);

        /// <summary>
        /// Clears the underlying data usng a specified color
        /// </summary>
        public void Clear(Color color = default) => pixels.Clear(color);


        /// <summary>
        /// Update the neopixels with the current data
        /// </summary>
        public void Update() => device.Write(pixels.GetData());
    }
}
