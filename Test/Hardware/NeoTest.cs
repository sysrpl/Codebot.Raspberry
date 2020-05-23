using System;
using Raspberry;
using Raspberry.Common;

namespace Test.Hardware
{
    public class NeoTest
    {
        private GpioPin pin;

        public NeoTest(int pin)
        {
            this.pin = Pi.Gpio.Pin(pin);
            this.pin.Mode = GpioPinMode.Output;
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("LED on");
                Pi.Wait(1000);
                this.pin.Write(true);
                Console.WriteLine("LED ff");
                Pi.Wait(1000);
                this.pin.Write(false);
            }
        }

        double cycle = (1d / 1_000_000d) * 1.25d;
        double zero = (1d / 1_000_000d) * 0.4d;
        double one = (1d / 1_000_000d) * 0.8d;
        double start;
        double finish;

        void White()
        {
            for (int i = 0; i < 24; i++)
            {
                /*pin.Write(true);
                if (i < 8)
                    while (Timer.Now < start + one) { }
                else
                    while (Timer.Now < start + zero) { }
                pin.Write(false);
                while (Timer.Now < finish) { }
                start = finish;
                finish = finish + cycle;*/
                pin.Write(true);
                Pi.WaitMicroseconds(0.01);
                pin.Write(false);
                Pi.WaitMicroseconds(0.01);

            }
        }

        void Black()
        {
            for (int i = 0; i < 24; i++)
            {
                pin.Write(true);
                while (Timer.Now < start + zero) { }
                pin.Write(false);
                while (Timer.Now < finish) { }
                start = finish;
                finish = finish + cycle;
            }
        }

        /// <summary>
        /// Turn on n number of neopixels 
        /// </summary>
        public void TurnOn(int n)
        {
            start = Timer.Now;
            finish = start + cycle;
            while (n > 0)
            {
                White();
                n--; 
            }
            Pi.Wait(1);
        }

        /// <summary>
        /// Turn off n number of neopixels 
        /// </summary>
        public void TurnOff(int n)
        {
            start = Timer.Now;
            finish = start + cycle;
            while (n > 0)
            {
                Black();
                n--;
            }
            Pi.Wait(1);
        }
    }
}
