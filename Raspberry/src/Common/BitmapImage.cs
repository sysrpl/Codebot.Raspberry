// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;

namespace Raspberry.Common
{
    /// <summary>
    /// Represents bitmap image
    /// </summary>
    public class BitmapImage
    {
        private const int BytesPerComponent = 3;
        private const int BytesPerPixel = BytesPerComponent * 3;
        // The Neo Pixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
        // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
        // data means we have to add 30 bytes of zero padding.
        private const int ResetDelayInBytes = 30;

        /// <summary>
        /// Initializes a <see cref="T:Iot.Device.Graphics.BitmapImage" /> instance with the specified data, width, height and stride.
        /// </summary>
        /// <param name="width">Width of the image</param>
        public BitmapImage(int width)
        {
            _data = new byte[width * BytesPerPixel + ResetDelayInBytes];
            Width = width;
        }

        private readonly byte[] _data;

        /// <summary>
        /// Data related to the image (derived class defines a specific format)
        /// </summary>
        public Span<byte> Data => _data;

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Sets pixel at specific position
        /// </summary>
        /// <param name="x">X coordinate of the pixel</param>
        /// <param name="y">Y coordinate of the pixel</param>
        public void SetPixel(int x, Color color)
        {
            var offset = x * BytesPerPixel;
            Data[offset++] = lookup[color.G * BytesPerComponent + 0];
            Data[offset++] = lookup[color.G * BytesPerComponent + 1];
            Data[offset++] = lookup[color.G * BytesPerComponent + 2];
            Data[offset++] = lookup[color.R * BytesPerComponent + 0];
            Data[offset++] = lookup[color.R * BytesPerComponent + 1];
            Data[offset++] = lookup[color.R * BytesPerComponent + 2];
            Data[offset++] = lookup[color.B * BytesPerComponent + 0];
            Data[offset++] = lookup[color.B * BytesPerComponent + 1];
            Data[offset++] = lookup[color.B * BytesPerComponent + 2];
        }

        private static readonly byte[] lookup = new byte[256 * BytesPerComponent];
        static BitmapImage()
        {
            for (int i = 0; i < 256; i++)
            {
                int data = 0;
                for (int j = 7; j >= 0; j--)
                {
                    data = (data << 3) | 0b100 | ((i >> j) << 1) & 2;
                }

                lookup[i * BytesPerComponent + 0] = unchecked((byte)(data >> 16));
                lookup[i * BytesPerComponent + 1] = unchecked((byte)(data >> 8));
                lookup[i * BytesPerComponent + 2] = unchecked((byte)(data >> 0));
            }
        }

        /// <summary>
        /// Clears the image to specific color
        /// </summary>
        /// <param name="color">Color to clear the image. Defaults to black.</param>
        public virtual void Clear(Color color = default)
        {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, color);
                }
        }
    }
}
