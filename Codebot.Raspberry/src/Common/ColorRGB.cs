using System;
// using System.Drawing;

namespace Codebot.Raspberry.Common
{
    public struct ColorRGB : IEquatable<ColorRGB>, IEquatable<int>
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public static readonly ColorRGB Black = new ColorRGB(0, 0, 0);
        public static readonly ColorRGB White = new ColorRGB(255, 255, 255);
        public static readonly ColorRGB Red = new ColorRGB(255, 0, 0);

        public override bool Equals(object obj)
        {
            if (obj is ColorRGB)
                return obj.GetHashCode() == GetHashCode();
            if (obj is int)
                return obj.GetHashCode() == GetHashCode();
            return false;
        }

        public static bool operator ==(ColorRGB a, ColorRGB b)
        {
            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(ColorRGB a, ColorRGB b)
        {
            return a.GetHashCode() != b.GetHashCode();
        }

        public static implicit operator int(ColorRGB rgb)
        {
            return rgb.GetHashCode();
        }

        public static explicit operator ColorRGB(int rgb)
        {
            return new ColorRGB(rgb);
        }

        public static ColorRGB operator +(ColorRGB colorA, ColorRGB colorB)
        {
            int r = colorA.R + colorB.R;
            int g = colorA.G + colorB.G;
            int b = colorA.B + colorB.B;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return new ColorRGB((byte)r, (byte)g, (byte)b);
        }

        public static ColorRGB operator -(ColorRGB colorA, ColorRGB colorB)
        {
            int r = colorA.R - colorB.R;
            int g = colorA.G - colorB.G;
            int b = colorA.B - colorB.B;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            return new ColorRGB((byte)r, (byte)g, (byte)b);
        }

        public static ColorRGB operator *(ColorRGB a, double f)
        {
            return a.Scale(f);
        }

        public ColorRGB(int rgb)
        {
            R = (byte)(rgb >> 16 & 0xFF);
            G = (byte)(rgb >> 8 & 0xFF);
            B = (byte)(rgb & 0xFF);
        }

        public ColorRGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public (byte, byte, byte) ToBytes()
        {
            return (R, G, B);
        }

        public static ColorRGB FromBytes(byte r, byte g, byte b)
        {
            return new ColorRGB(r, g, b);
        }

        public static ColorRGB FromString(string s)
        {
            var color = 0;
            var i = 0;
            var j = 0;
            foreach (var c in s.ToUpper())
            {
                if (i == 0 && c == '#')
                    continue;
                if (c >= '0' && c <= '9')
                {
                    color = (color << 4) | (byte)(c - '0');
                }
                else if (c >= 'A' && c <= 'F')
                {
                    color = (color << 4) | (byte)(c - 'A' + 10);
                }
                if (j++ > 5)
                    return Black;
            }
            return (ColorRGB)color;
        }

        public override int GetHashCode()
        {
            return R << 16 | G << 8 | B;
        }

        public bool Equals(ColorRGB other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public bool Equals(int other)
        {
            return GetHashCode() == other;
        }

        public ColorRGB Scale(double s)
        {
            if (s < 0)
                return Black;
            int r = (int)Math.Round(R * s);
            int g = (int)Math.Round(G * s);
            int b = (int)Math.Round(B * s);
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return new ColorRGB((byte)r, (byte)g, (byte)b);
        }

        public ColorRGB Scale(double sr, double sg, double sb)
        {
            int r = (int)Math.Round(R * sr);
            int g = (int)Math.Round(G * sg);
            int b = (int)Math.Round(B * sb);
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return new ColorRGB((byte)r, (byte)g, (byte)b);
        }

        /*public struct ColorRGB
        {
            public byte R;
            public byte G;
            public byte B;

            public ColorRGB(Color value)
            {
                R = value.R;
                G = value.G;
                B = value.B;
            }

            public static implicit operator Color(ColorRGB rgb) => Color.FromArgb(rgb.R, rgb.G, rgb.B);
            public static explicit operator ColorRGB(Color c) => new ColorRGB(c);

            public static ColorRGB FromHSL(double H, double S, double L)
            {
                if (H < 0)
                    H = 1 + H % 1;
                else
                    H %= 1;
                if (H > 0.9999)
                    H = 0;
                if (S < 0)
                    S = 0;
                else if (S > 1)
                    S = 1;
                if (L < 0)
                    L = 0;
                else if (L > 1)
                    L = 1;
                double v;
                double r, g, b;
                r = L;
                g = L;
                b = L;
                v = (L <= 0.5) ? (L * (1.0 + S)) : (L + S - L * S);
                if (v > 0)
                {
                    double m;
                    double sv;
                    int sextant;
                    double f, vsf, mid1, mid2;
                    m = L + L - v;
                    sv = (v - m) / v;
                    H *= 6.0;
                    sextant = (int)H;
                    f = H - sextant;
                    vsf = v * sv * f;
                    mid1 = m + vsf;
                    mid2 = v - vsf;
                    switch (sextant)
                    {
                        case 0:
                            r = v;
                            g = mid1;
                            b = m;
                            break;
                        case 1:
                            r = mid2;
                            g = v;
                            b = m;
                            break;
                        case 2:
                            r = m;
                            g = v;
                            b = mid1;
                            break;
                        case 3:
                            r = m;
                            g = mid2;
                            b = v;
                            break;
                        case 4:
                            r = mid1;
                            g = m;
                            b = v;
                            break;
                        case 5:
                            r = v;
                            g = m;
                            b = mid2;
                            break;
                    }
                }
                return new ColorRGB
                {
                    R = Convert.ToByte(r * 255),
                    G = Convert.ToByte(g * 255),
                    B = Convert.ToByte(b * 255)
                };
            }

            public float H
            {
                get
                {
                    return ((Color)this).GetHue() / 360.0F;
                }
            }

            public float S
            {
                get
                {
                    return ((Color)this).GetSaturation();
                }
            }

            public float L
            {
                get
                {
                    return ((Color)this).GetBrightness();
                }
            }
        }

        public static class ColorExtension
        {
            public static Color Mix(this Color color, Color value, double percent)
            {
                if (percent < 0.001)
                    return color;
                if (percent > 0.999)
                    return value;
                var i = 1 - percent;
                var r = (int)Math.Round(value.R * percent + color.R * i);
                var g = (int)Math.Round(value.G * percent + color.G * i);
                var b = (int)Math.Round(value.B * percent + color.B * i);
                return Color.FromArgb(r, g, b);
            }

            public static Color FromHue(this Color color, double h)
            {
                return ColorRGB.FromHSL(h, 1, 0.5);
            }

            public static Color Hue(this Color color, double h)
            {
                var c = new ColorRGB(color);
                return ColorRGB.FromHSL(h, c.S, c.L);
            }

            public static Color Saturation(this Color color, double s)
            {
                var c = new ColorRGB(color);
                return ColorRGB.FromHSL(c.H, s, c.L);
            }

            public static Color Lightness(this Color color, double l)
            {
                var c = new ColorRGB(color);
                return ColorRGB.FromHSL(c.H, c.S, l);
            }
        }*/
    }
}