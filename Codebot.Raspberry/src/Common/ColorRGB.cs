using System;
using System.Drawing;

namespace Codebot.Raspberry.Common
{
    public struct ColorRGB
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
                H = H % 1;
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
}
