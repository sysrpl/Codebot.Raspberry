﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// Buzzer is a generic buzzer class
    /// </summary>
    public class Buzzer : PwmOutput
    {
        /// <summary>
        /// Buzzer constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public Buzzer(GrovePi grovePi, GrovePort port)
            : base(grovePi, port)
        {
        }

        /// <summary>
        /// Get the name Buzzer
        /// </summary>
        public new string SensorName => "Buzzer";
    }
}
