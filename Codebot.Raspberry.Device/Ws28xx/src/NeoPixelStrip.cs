using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Codebot.Raspberry.Board.Spi;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// The WS28XX device class represents a strip of WS28XX NeoPixels
    /// </summary>
    /// <remarks>This device communicates with WS28XX NeoPixels using SPI over GPIO 10</remarks>
    [Device("WS28XX", "NeoPixel Strip", Category = "Lighting", Remarks = "Must use GPIO 10")]
    public sealed class NeoPixelStrip : HardwareDevice, IDisposable, IEnumerable<NeoPixel>
    {
        /// <summary>
        /// Private class used to manage NeoPixelStrip strip data
        /// </summary>
        private class PixelData
        {
            private const int BytesPerComponent = 3;
            private const int BytesPerPixel = BytesPerComponent * 3;
            private static readonly byte[] lookup = new byte[256 * BytesPerComponent];

            static PixelData()
            {
                for (int i = 0; i < 256; i++)
                {
                    int data = 0;
                    for (int j = 7; j >= 0; j--)
                        data = (data << 3) | 0b100 | ((i >> j) << 1) & 2;
                    lookup[i * BytesPerComponent + 0] = unchecked((byte)(data >> 16));
                    lookup[i * BytesPerComponent + 1] = unchecked((byte)(data >> 8));
                    lookup[i * BytesPerComponent + 2] = unchecked((byte)(data >> 0));
                }
            }

            // The NeoPixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
            // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
            // data means we have to add 30 bytes of zero padding.
            private const int ResetDelayInBytes = 30;

            private byte[] data;
            public Span<byte> Data => data;

            public void Resize(int count)
            {
                data = new byte[count * BytesPerPixel + ResetDelayInBytes];
            }

            public void SetPixel(int index, Color color)
            {
                var offset = index * BytesPerPixel;
                data[offset++] = lookup[color.G * BytesPerComponent + 0];
                data[offset++] = lookup[color.G * BytesPerComponent + 1];
                data[offset++] = lookup[color.G * BytesPerComponent + 2];
                data[offset++] = lookup[color.R * BytesPerComponent + 0];
                data[offset++] = lookup[color.R * BytesPerComponent + 1];
                data[offset++] = lookup[color.R * BytesPerComponent + 2];
                data[offset++] = lookup[color.B * BytesPerComponent + 0];
                data[offset++] = lookup[color.B * BytesPerComponent + 1];
                data[offset++] = lookup[color.B * BytesPerComponent + 2];
            }
        }

        private readonly PixelData data;
        private readonly SpiDevice device;
        private readonly List<NeoPixel> pixels;

        /// <summary>
        /// Create a new strip of count neopixels
        /// </summary>
        public NeoPixelStrip(int count)
        {
            data = new PixelData();
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 2_400_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            };
            device = SpiDevice.Create(settings);
            pixels = new List<NeoPixel>();
            Count = count;
        }

        /// <summary>
        /// Get or set the number of neopixels in the strip
        /// </summary>
        public int Count
        {
            get => pixels.Count;
            set
            {
                if (value == pixels.Count)
                    return;
                while (pixels.Count < value)
                    pixels.Add(new NeoPixel());
                if (pixels.Count > value)
                    pixels.RemoveRange(pixels.Count, value - pixels.Count);
                data.Resize(value);
                NeoPixel p;
                for (var i = 0; i < pixels.Count; i++)
                {
                    p = pixels[i];
                    data.SetPixel(i, p.Color);
                    p.Changed = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets a neopixel by index
        /// </summary>
        public NeoPixel this[int index]
        {
            get => pixels[index];
            set => pixels[index].Color = value.Color;
        }

        /// <summary>
        /// Turn of all neopixels
        /// </summary>
        public void Reset()
        {
            foreach (var p in pixels)
                p.Color = Color.Black;
        }

        /// <summary>
        /// Send color information to all the neopixels in the strip
        /// </summary>
        public override bool Update()
        {
            NeoPixel p;
            for (var i = 0; i < pixels.Count; i++)
            {
                p = pixels[i];
                if (p.Changed)
                {
                    data.SetPixel(i, p.Color);
                    p.Changed = false;
                }
            }
            device.Write(data.Data);
            return true;
        }

        #region interfaces
        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
                device.Dispose();
            disposed = true;
        }

        public IEnumerator<NeoPixel> GetEnumerator()
        {
            return ((IEnumerable<NeoPixel>)pixels).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<NeoPixel>)pixels).GetEnumerator();
        }
        #endregion
    }
}
