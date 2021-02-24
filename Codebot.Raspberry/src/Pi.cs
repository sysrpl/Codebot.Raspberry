using System;
using System.IO;
using System.Collections.Generic;
using Codebot.Raspberry.Common;
using Codebot.Raspberry.Board;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The Pi class provide users access to Gpio pins and Wait methods
    /// </summary>
    public static class Pi
    {
        static PreciseTimer now;

        public static double Now => now.ElapsedMilliseconds;

        static void Init()
        {
            now = new PreciseTimer();
            WaitMicroseconds(1);
            var m = now.ElapsedMicroseconds;
        }

        static Pi()
        {
            Init();
        }

        /// <summary>
        /// Returns a number of nanoseconds as a fraction of milliseconds
        /// </summary>
        public static double Nanoseconds(double nanoseconds)
        {
            return nanoseconds / 1000000d;
        }

        /// <summary>
        /// Returns a number of microseconds as a fraction of milliseconds
        /// </summary>
        public static double Microseconds(double microseconds)
        {
            return microseconds / 1000d;
        }

        /// <summary>
        /// Wait a specified number of nanoseconds
        /// <remarks>A nanosecond is a billionth of a second</remarks>
        /// </summary>
        public static void WaitNanoseconds(double nanoseconds)
        {
            Wait(Nanoseconds(nanoseconds));
        }

        /// <summary>
        /// Wait a specified number of milliseconds
        /// <remarks>A microsecond is a millionth of a second</remarks>
        /// </summary>
        public static void WaitMicroseconds(double microseconds)
        {
            Wait(Microseconds(microseconds));
        }

        /// <summary>
        /// Wait a specified number of milliseconds
        /// <remarks>A millisecond is a thousandth of a second</remarks>
        /// </summary>
        public static void Wait(double milliseconds)
        {
            var timer = new PreciseTimer();
            double seconds = milliseconds / 1000;
            while (seconds - timer.ElapsedSeconds > 1)
                System.Threading.Thread.Sleep(100);
            while (seconds - timer.ElapsedSeconds > 0.1)
                System.Threading.Thread.Sleep(10);
            while (seconds - timer.ElapsedSeconds > 0.01)
                System.Threading.Thread.Sleep(1);
            while (seconds - timer.ElapsedSeconds > 0)
                { }
        }

        /// <summary>
        /// The CPU temperature of this computer
        /// </summary>
        public static Temperature CpuTemperature
        {
            get
            {
                const string fileName = "/sys/class/thermal/thermal_zone0/temp";
                var t = double.NaN;
                if (File.Exists(fileName))
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string data = reader.ReadLine();
                        if (!string.IsNullOrEmpty(data))
                            if (int.TryParse(data, out int temp))
                                t = temp / 1000F;
                    }
                return Temperature.FromCelsius(t);
            }
        }

        /// <summary>
        /// The Gpio class provide users access to the Raspberry Pi Gpio pins
        /// </summary>
        public static class Gpio
        {
            internal static readonly GpioController controller;
            private static readonly GpioPin[] pins;

            static Gpio()
            {
                Pi.Init();
                controller = new GpioController(PinNumberingScheme.Logical);
                pins = new GpioPin[30];
                for (int i = 0; i < pins.Length; i++)
                    pins[i] = null;
            }

            static internal void Close(int number)
            {
                if (number < 0)
                    number = 0;
                if (number > pins.Length - 1)
                    number = pins.Length - 1;
                pins[number] = null;
            }

            /// <summary>
            /// Draw a diagram of the pin layout
            /// </summary>
            public static void Diagram()
            {
                Console.WriteLine("+--------------------------------------+");
                Console.WriteLine("| 1  3v3 Power      | 2  5v Power      |");
                Console.WriteLine("| 3  BCM 2 (SDA)    | 4  5v Power      |");
                Console.WriteLine("| 5  BCM 3 (SCL)    | 6  Ground        |");
                Console.WriteLine("| 7  BCM 4 (GPCLK0) | 8  BCM 14 (TXD)  |");
                Console.WriteLine("| 9  Ground         | 10 BCM 15 (RXD)  |");
                Console.WriteLine("| 11 BCM 17         | 12 BCM 18 (PWM0) |");
                Console.WriteLine("| 13 BCM 27         | 14 Ground        |");
                Console.WriteLine("| 15 BCM 22         | 16 BCM 23        |");
                Console.WriteLine("| 17 3v3 Power      | 18 BCM 24        |");
                Console.WriteLine("| 19 BCM 10 (MOSI)  | 20 Ground        |");
                Console.WriteLine("| 21 BCM 9 (MISO)   | 22 BCM 25        |");
                Console.WriteLine("| 23 BCM 11 (SCLK)  | 24 BCM 8 (CE0)   |");
                Console.WriteLine("| 25 Ground         | 26 BCM 7 (CE1)   |");
                Console.WriteLine("| 27 (ID_SD)        | 28 (ID_SC)       |");
                Console.WriteLine("| 29 BCM 5          | 30 Ground        |");
                Console.WriteLine("| 31 BCM 6          | 32 BCM 12 (PWM0) |");
                Console.WriteLine("| 33 BCM 13 (PWM1)  | 34 Ground        |");
                Console.WriteLine("| 35 BCM 19 (MISO)  | 36 BCM 16        |");
                Console.WriteLine("| 37 BCM 26         | 38 BCM 20 (MOSI) |");
                Console.WriteLine("| 39 Ground         | 40 BCM 21 (SCLK) |");
                Console.WriteLine("+--------------------------------------+");
            }

            /// <summary>
            /// The Gpio pins.
            /// </summary>
            public static IEnumerable<GpioPin> Pins
            {
                get
                {
                    foreach (var pin in pins)
                    {
                        if (pin == null)
                            continue;
                        if (pin.Valid)
                            yield return pin;
                    }
                }
            }

            /// <summary>
            /// The list of GpioPin names.
            /// </summary>
            public static IEnumerable<string> Names
            {
                get
                {
                    foreach (var pin in pins)
                    {
                        if (pin == null)
                            continue;
                        if (pin.Valid)
                            yield return pin.Name;
                    }
                }
            }

            /// <summary>
            /// Get a GpioPin using its logical (Gpio) number
            /// </summary>
            public static GpioPin Pin(int number)
            {

                if (number < 0)
                    number = 0;
                if (number > pins.Length - 1)
                    number = pins.Length - 1;
                var pin = pins[number];
                if (pin == null)
                {
                    pin = new GpioPin(controller, number);
                    pins[number] = pin;
                    if (pin.Valid)
                        controller.OpenPin(number);
                }
                return pin;
            }

            /// <summary>
            /// Get a GpioPin using its logical (Gpio) number and set it to
            /// a specified pin kind.
            /// </summary>
            public static GpioPin Pin(int number, PinKind kind)
            {
                var pin = Pin(number);
                pin.Kind = kind;
                return pin;
            }

            /// <summary>
            /// Gets the name of a pin based on its logical (Gpio) number
            /// </summary>
            public static string Name(int number)
            {
                switch (number)
                {
                    case 2:
                        return "BCM 2 (SDA)";
                    case 3:
                        return "BCM 3 (SCL)";
                    case 4:
                        return "BCM 4 (GPCLK0)";
                    case 5:
                        return "BCM 5";
                    case 6:
                        return "BCM 6";
                    case 7:
                        return "BCM 7 (CE1)";
                    case 8:
                        return "BCM 8 (CE0)";
                    case 9:
                        return "BCM 9 (MISO)";
                    case 10:
                        return "BCM 10 (MOSI)";
                    case 11:
                        return "BCM 11 (SCLK)";
                    case 12:
                        return "BCM 12 (PWM0)";
                    case 13:
                        return "BCM 13 (PWM1)";
                    case 14:
                        return "BCM 14 (TXD)";
                    case 15:
                        return "BCM 15 (RXD)";
                    case 16:
                        return "BCM 16";
                    case 17:
                        return "BCM 17";
                    case 18:
                        return "BCM 18 (PWM0)";
                    case 19:
                        return "BCM 19 (MISO)";
                    case 20:
                        return "BCM 20 (MOSI)";
                    case 21:
                        return "BCM 21 (SCLK)";
                    case 22:
                        return "BCM 22";
                    case 23:
                        return "BCM 23";
                    case 24:
                        return "BCM 24";
                    case 25:
                        return "BCM 25";
                    case 26:
                        return "BCM 26";
                    case 27:
                        return "BCM 27";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}