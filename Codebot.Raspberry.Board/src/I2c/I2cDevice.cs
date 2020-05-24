﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Codebot.Raspberry.Board.I2c
{
    /// <summary>
    /// The communications channel to a device on an I2C bus.
    /// </summary>
    public abstract class I2cDevice : IDisposable
    {
        /// <summary>
        /// The connection settings of a device on an I2C bus. The connection settings are immutable after the device is created
        /// so the object returned will be a clone of the settings object.
        /// </summary>
        public abstract I2cConnectionSettings ConnectionSettings { get; }

        /// <summary>
        /// Reads a byte from the I2C device.
        /// </summary>
        /// <returns>A byte read from the I2C device.</returns>
        public abstract byte ReadByte();

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public abstract void Read(Span<byte> buffer);

        /// <summary>
        /// Writes a byte to the I2C device.
        /// </summary>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public abstract void WriteByte(byte value);

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public abstract void Write(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the I2C bus on which the device is connected,
        /// and sends a restart condition between the write and read operations.
        /// </summary>
        /// <param name="writeBuffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.</param>
        /// <param name="readBuffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public abstract void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer);

        /// <summary>
        /// Creates a communications channel to a device on an I2C bus running on the current platform
        /// </summary>
        /// <param name="settings">The connection settings of a device on an I2C bus.</param>
        /// <returns>A communications channel to a device on an I2C bus running on Windows 10 IoT.</returns>
        public static I2cDevice Create(I2cConnectionSettings settings)
        {
            return new UnixI2cDevice(settings);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if explicitly disposing, <see langword="false"/> if in finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }
    }
}
