using System;
using System.Drawing;

namespace Codebot.Raspberry.Common
{
    public class HSL
    {
        private double hue = 1;
        private double saturation = 1;
        private double luminosity = 1;
        const double scale = 240;

        public HSL() { }

        public HSL(Color color)
        {
            RGB(color.R, color.G, color.B);
        }

        public HSL(byte red, byte green, byte blue)
        {
            RGB(red, green, blue);
        }

        public HSL(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

        public static implicit operator HSL(Color color)
        {
            return new HSL
            {
                hue = color.GetHue() / 360.0,
                luminosity = color.GetBrightness(),
                saturation = color.GetSaturation()
            };
        }

        public static implicit operator Color(HSL hslColor)
        {
            const double EPSILON = 0.0001;
            double r = 0, g = 0, b = 0;
            if (Math.Abs(hslColor.luminosity) > EPSILON)
            {
                if (Math.Abs(hslColor.saturation) < EPSILON)
                    r = g = b = hslColor.luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.luminosity - temp2;
                    r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.hue);
                    b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        public void RGB(byte red, byte green, byte blue)
        {
            HSL hsl = Color.FromArgb(red, green, blue);
            hue = hsl.hue;
            saturation = hsl.saturation;
            luminosity = hsl.luminosity;
        }

        public double Hue
        {
            get => hue * scale; 
            set { hue = CheckRange(value / scale); }
        }

        public double Saturation
        {
            get => saturation * scale; 
            set { saturation = CheckRange(value / scale); }
        }
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            if (temp3 < 0.5)
                return temp2;
            if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            return temp1;
        }

        static double MoveIntoRange(double t)
        {
            if (t < 0)
                t += 1;
            else if (t > 1)
                t -= 1;
            return t;
        }

        static double GetTemp2(HSL hsl)
        {
            double t;
            if (hsl.luminosity < 0.5)  
                t = hsl.luminosity * (1.0 + hsl.saturation);
            else
                t = hsl.luminosity + hsl.saturation - (hsl.luminosity * hsl.saturation);
            return t;
        }
    }
}
