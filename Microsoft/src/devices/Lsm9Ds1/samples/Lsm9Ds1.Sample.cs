﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Device.I2c;

namespace Iot.Device.Lsm9Ds1.Samples
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // uncomment to run accelerometer sample
            // AccelerometerAndGyroscope.Run();
            Magnetometer.Run();
        }
    }
}
