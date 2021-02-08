using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class WeatherTest
    {
        public static void Run()
        {
            var pinNumber = 4;
            Console.WriteLine($"Weather Test on GPIO {pinNumber}");
            var sensor = new Dht22(pinNumber);
            var attempts = 0;
            while (true)
            {
                Pi.Wait(2500);
                if (sensor.Update())
                {
                    Console.WriteLine($"The current temperature is {sensor.Temperature.Fahrenheit} " +
                            $"and the humidity is {sensor.Humidity}%");
                    Console.WriteLine($"Attempts since last update {attempts}");
                    attempts = 0;
                }
                else
                    attempts++;
            }
        }
    }
}
