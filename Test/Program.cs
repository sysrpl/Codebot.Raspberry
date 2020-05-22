using System;
using Raspberry;
using Raspberry.Device;
using Test.Hardware;

namespace Test
{
    class Program
    {
        static void Blink()
        {

        }

        static void Display()
        {
            Console.WriteLine("Testing LCD");
            var lcd = new CharacterLcd(23, 24, 25, 8, 7, 12);
            var console = new ConsoleLcd(lcd);
            var s = "Hello\nWorld!";
            while (s.Length > 0)
            {
                console.WriteLine(s);
                Console.WriteLine("Type some text to display or ENTER to quit:");
                s = Console.ReadLine().Trim();
            }
        }

        static void Weather()
        {
            Console.WriteLine("Testing the DHT22 sensor");
            var sensor = new Dht22(16);
            Console.WriteLine("Waiting for sensor to respond ...");
            Console.WriteLine("");
            while (!sensor.Update())
            {
            }
            var c = 0d;
            var f = 0d;
            var h = 0d;
            while (true)
            {
                if (sensor.Update())
                {
                    c = sensor.Temperature.Celsius;
                    f = sensor.Temperature.Fahrenheit;
                    h = sensor.Humidity;
                }
                var t = Pi.CpuTemperature;
                Console.WriteLine($"{DateTime.Now}");
                Console.WriteLine($"CPU: {t.Celsius:0.0}\u00B0C {t.Fahrenheit:0}\u00B0F");
                Console.WriteLine($"Temperature: {c:0.0}\u00B0C {f:0}\u00B0F");
                Console.WriteLine($"Humidity: {h:0.0}%");
                Console.WriteLine($"");
                Pi.Wait(1000);
            }
        }

        static void Neopixels()
        {

            Console.WriteLine("What GPIO pin do you want to use for neopixels?");
            var input = Console.ReadLine().Trim();
            var pin = 18;
            if (int.TryParse(input, out pin) && (Pi.Gpio.Pin(pin).Valid))
            {
                var n = new NeoTest(pin);
                Console.WriteLine("How many neopixels do you want to turn on?");
                input = Console.ReadLine().Trim();
                int count;
                if (int.TryParse(input, out count) && (count > 0) && (count < 100))
                {
                    n.TurnOn(count);
                    Console.WriteLine("Press enter to quit");
                    Console.ReadLine();
                    Console.WriteLine("Turning off pixels");
                    n.TurnOff(count);
                }
                else
                 Console.WriteLine($"'{input}' is not a valid number of neopixels");

            }
            else
                Console.WriteLine($"'{input}' is not a valid pin");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Raspberry Pi Testing");
            Pi.Gpio.Diagram();
            Neopixels();
            // Display();
            // Weather();
        }
    }
}
