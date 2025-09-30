using System.Drawing;
using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
{
    public class NeoPixel
    {
        private static readonly NeoPixel convert = new NeoPixel();

        public static NeoPixel FromColor(Color c)
        {
            convert.Color = c;
            return convert;
        }

        public static implicit operator Color (NeoPixel p) => p.color;
        public static implicit operator ColorRGB (NeoPixel p) => new ColorRGB(p.color);

        public static implicit operator NeoPixel (Color c) => FromColor(c);
        public static implicit operator NeoPixel (ColorRGB c) => FromColor(c);

        public NeoPixel() : this(Color.Black)
        {
        }

        public NeoPixel(Color c)
        {
            color = c;
            Changed = true;
        }

        public bool Changed { get; set; }

        private Color color;
        private double hue;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                hue = color.GetHue() / 360d;
                Changed = true;
            }
        }

        public int Data { get; set; }

        public NeoPixel Mix(Color a, Color b, double m)
        {
            if (m <= 0)
                Color = a;
            else if (m >= 1)
                Color = b;
            else
            {
                var i = 1 - m;
                int red = (int)(a.R * i + b.R * m);
                int blue = (int)(a.B * i + b.B * m);
                int green = (int)(a.G * i + b.G * m);
                Color = Color.FromArgb(red, green, blue);
            }
            return this;
        }

        public NeoPixel Hue(double h)
        {
            Color = ColorRGB.FromHSL(h, 1, 0.5);
            return this;
        }

        public NeoPixel Lightness(double l)
        {
            Color = ColorRGB.FromHSL(hue, 1, l);
            return this;
        }

        public NeoPixel Saturation(double s)
        {
            Color = ColorRGB.FromHSL(hue, s, 0.5);
            return this;
        }

        public NeoPixel Rgb(int color)
        {
            Color = Color.FromArgb(color | (0xFF << 24));
            return this;
        }

        public NeoPixel Rgb(byte r, byte g, byte b)
        {
            Color = Color.FromArgb(r, g, b);
            return this;
        }
    }
}
