using System;
using System.Drawing;

namespace Raspberry.Device
{
    internal class Pixels
    {
        const int BytesPerComponent = 3;
        const int BytesPerPixel = BytesPerComponent * 3;
        // The Neo Pixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
        // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
        // data means we have to add 30 bytes of zero padding.
        const int ResetDelayInBytes = 30;
        static readonly byte[] lookup = new byte[256 * BytesPerComponent];

        static Pixels()
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

        public Pixels(int count)
        {
            Count = count;
        }

        public Color this[int index]
        {
            get
            {
                return Color.Black;
            }
            set
            {
                SetPixel(index, value);
            }
        }

        private byte[] data;

        public Span<byte> GetData()
        {
            return data;
        }

        private int count;
        public int Count
        {
            get => count;
            set
            {
                if (value == count)
                    return;
                count = value;
                data = new byte[count * BytesPerPixel + ResetDelayInBytes];
                Clear();
            }
        }

        public void SetPixel(int index, Color color)
        {
            var offset = index * BytesPerPixel;
            data[offset++] = lookup[color.G * BytesPerComponent + 0];
            data[offset++] = lookup[color.G * BytesPerComponent + 1];
            data[offset++] = lookup[color.R * BytesPerComponent + 0];
            data[offset++] = lookup[color.R * BytesPerComponent + 1];
            data[offset++] = lookup[color.R * BytesPerComponent + 2];
            data[offset++] = lookup[color.B * BytesPerComponent + 0];
            data[offset++] = lookup[color.B * BytesPerComponent + 1];
            data[offset++] = lookup[color.B * BytesPerComponent + 2];
        }

        public virtual void Clear(Color color = default)
        {
            for (int i = 0; i < Count; i++)
                SetPixel(i, color);
        }
    }
}
