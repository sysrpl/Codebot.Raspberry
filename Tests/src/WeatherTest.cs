using System;
using Codebot.Raspberry;
using Codebot.Raspberry.Device;

namespace Tests
{
    public static class WeatherTest
    {
        public static void Run()
        {
            var pinNumber = 22;
            Console.WriteLine($"Weather Test on GPIO {pinNumber}");
            var sensor = new Dht22(pinNumber);
            while (true)
            {
                if (sensor.Update())
                    Console.WriteLine($"The current temperature is {sensor.Temperature.Fahrenheit} " +
                        $"and the humdity is {sensor.Humidity}%");
                else
                    Console.WriteLine("Could not read weather data");
                Pi.Wait(5000);
            }
        }
    }
}
