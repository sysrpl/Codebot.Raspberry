using System;
using System.Drawing;

namespace Raspberry.Device
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

        public Color Color
        {
            get => color;
            set 
            {
                color = value;
                Changed = true;
            }
        }
    }
}
