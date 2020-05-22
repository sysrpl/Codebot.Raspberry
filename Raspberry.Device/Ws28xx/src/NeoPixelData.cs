using System;
using System.Drawing;

namespace Raspberry.Device
{
    internal class NeoPixelData
    {
        const int BytesPerComponent = 3;
        const int BytesPerPixel = BytesPerComponent * 3;
        static readonly byte[] lookup = new byte[256 * BytesPerComponent];

        static NeoPixelData()
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

        // The Neo Pixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
        // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
        // data means we have to add 30 bytes of zero padding.
        private const int ResetDelayInBytes = 30;

        public NeoPixelData(int count)
        {
            data = new byte[count * BytesPerPixel + ResetDelayInBytes];
        }

        readonly byte[] data;
        public Span<byte> Data => data;

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
}
