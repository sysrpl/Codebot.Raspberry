using System.Drawing;
using Codebot.Raspberry.Common;

namespace Codebot.Raspberry.Device
{
    public class NeoPixel
    {
        static NeoPixel convert = new NeoPixel();

        public static NeoPixel FromColor(Color c)
        {
            convert.Color = c;
            return convert;
        }

        public static implicit operator Color(NeoPixel p) => p.color;
        public static implicit operator NeoPixel(Color c) => FromColor(c);

        public NeoPixel() : this(Color.Black)
        { 
        }

        public NeoPixel(Color c)
        {
            color = c;
            Changed = true;
        }

        public bool Changed { get; set; }

        Color color;
        double hue;

        public Color Color
        {
            get => color;
            set 
            {
                color = value;
                hue = color.GetHue() / 360;
                Changed = true;
            }
        }

        public NeoPixel Hue(double h)
        {
            Color = new HSL(h, 1, 0.5);
            return this;
        }

        public NeoPixel Lightness(double l)
        {
            HSL hsl = Color;
            hsl.Hue = hue;
            hsl.Luminosity = l;
            color = hsl;
            Changed = true;
            return this;
        }

        public NeoPixel Saturation(double s)
        {
            HSL hsl = Color;
            hsl.Hue = hue;
            hsl.Saturation= s;
            color = hsl;
            Changed = true;
            return this;
        }

        public NeoPixel Rgb(int color)
        {
            Color = Color.FromArgb(color | (0xFF << 24));
            return this;
        }

        public NeoPixel Rgb(byte r, byte g, byte b)
        {
            Color = Color.FromArgb( r, g, b);
            return this;
        }
    }
}
