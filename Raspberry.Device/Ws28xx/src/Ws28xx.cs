// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Raspberry.Board.Spi;
using Raspberry.Common;

namespace Raspberry.Device
{
    /// <summary>
    /// Represents base class for WS28XX LED drivers (i.e. WS2812B or WS2808)
    /// </summary>
    public class Ws28xx
    {
        /// <summary>
        /// SPI device used for communication with the LED driver
        /// </summary>
        protected readonly SpiDevice _spiDevice;

        /// <summary>
        /// Backing image to be updated on the driver
        /// </summary>
        public BitmapImage Image { get; protected set; }

        /// <summary>
        /// Constructs Ws28xx instance
        /// </summary>
        /// <param name="spiDevice">SPI device used for communication with the LED driver</param>
        public Ws28xx(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        /// <summary>
        /// Sends backing image to the LED driver
        /// </summary>
        public void Update() => _spiDevice.Write(Image.Data);
    }
        /*static void TestWs28xx()
        {
            Console.WriteLine("Press return to start spi");
            Console.ReadLine();
            const int Count = 8;

            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 2_400_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };

            // Create a Neo Pixel x8 stick on spi 0.0
            var spi = SpiDevice.Create(settings);

            var neo = new Raspberry.Device.Ws2812b(spi, Count);
            BitmapImage img = neo.Image;
            img.Clear();
            img.SetPixel(0, 0, Color.White);
            img.SetPixel(1, 0, Color.Red);
            img.SetPixel(2, 0, Color.Green);
            img.SetPixel(3, 0, Color.Blue);
            img.SetPixel(4, 0, Color.Yellow);
            img.SetPixel(5, 0, Color.Cyan);
            img.SetPixel(6, 0, Color.Magenta);
            img.SetPixel(7, 0, Color.FromArgb(unchecked((int)0xffff8000)));
            neo.Update();
            Console.WriteLine("Press return to exit");
            Console.ReadLine();
            img.Clear();
            img.SetPixel(0, 0, Color.Black);
            img.SetPixel(1, 0, Color.Black);
            img.SetPixel(2, 0, Color.Black);
            img.SetPixel(3, 0, Color.Black);
            img.SetPixel(4, 0, Color.Black);
            img.SetPixel(5, 0, Color.Black);
            img.SetPixel(6, 0, Color.Black);
            img.SetPixel(7, 0, Color.Black);
            neo.Update();
        }*/
}
