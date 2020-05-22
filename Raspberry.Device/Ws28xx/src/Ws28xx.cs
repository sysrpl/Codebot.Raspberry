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
        protected readonly SpiDevice device;

        /// <summary>
        /// Backing image to be updated on the driver
        /// </summary>
        public BitmapImage Image { get; protected set; }

        /// <summary>
        /// Constructs Ws28xx instance
        /// </summary>
        public Ws28xx(int width)
        {
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 2_400_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };

            // Create a neopixel stick on spi 0.0
            device = SpiDevice.Create(settings);
            Image = new BitmapImage(width);
        }

        /// <summary>
        /// Sends backing image to the LED driver
        /// </summary>
        public void Update() => device.Write(Image.Data);
    }
}
