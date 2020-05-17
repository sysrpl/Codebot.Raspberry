﻿using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Ags01db.Samples
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Ags01db.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            using (Ags01db sensor = new Ags01db(device))
            {
                // read AGS01DB version
                Console.WriteLine($"Version: {sensor.Version}");
                Console.WriteLine();

                while (true)
                {
                    // read concentration
                    Console.WriteLine($"VOC Gas Concentration: {sensor.Concentration}ppm");
                    Console.WriteLine();

                    Thread.Sleep(3000);
                }
            }
        }
    }
}
