﻿using System.Drawing;
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
            Color = ColorRGB.FromHSL(h, 1, 0.5);
            return this;
        }

        public NeoPixel Lightness(double l)
        {
            Color = ColorRGB.FromHSL(Color.GetHue() / 360d, 1, l);
            return this;
        }

        public NeoPixel Saturation(double s)
        {
            Color = ColorRGB.FromHSL(Color.GetHue() / 360d, s, 0.5);
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
