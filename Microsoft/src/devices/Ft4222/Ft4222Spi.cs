﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Spi;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// Create a SPI Device based on FT4222 chipset
    /// </summary>
    public class Ft4222Spi : SpiDevice
    {
        private readonly SpiConnectionSettings _settings;
        private SafeFtHandle _ftHandle;

        /// <inheritdoc/>
        public override SpiConnectionSettings ConnectionSettings => _settings;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public DeviceInformation DeviceInformation { get; internal set; }

        /// <summary>
        /// Create an SPI FT4222 class
        /// </summary>
        /// <param name="settings">SPI Connection Settings</param>
        public Ft4222Spi(SpiConnectionSettings settings)
        {
            _settings = settings;
            // Check device
            var devInfos = FtCommon.GetDevices();
            if (devInfos.Count == 0)
            {
                throw new IOException("No FTDI device available");
            }

            // Select the one from bus Id
            // FT4222 propose depending on the mode multiple interfaces. Only the A is available for SPI or where there is none as it's the only interface
            var devInfo = devInfos.Where(m => m.Description == "FT4222 A" || m.Description == "FT4222").ToArray();
            if ((devInfo.Length == 0) || (devInfo.Length < _settings.BusId))
            {
                throw new IOException($"Can't find a device to open SPI on index {_settings.BusId}");
            }

            DeviceInformation = devInfo[_settings.BusId];
            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, out _ftHandle);

            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to open device {DeviceInformation.Description} with error: {ftStatus}");
            }

            // Set the clock but we need some math
            var (ft4222Clock, tfSpiDiv) = CalculateBestClockRate();

            ftStatus = FtFunction.FT4222_SetClock(_ftHandle, ft4222Clock);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to set clock rate {ft4222Clock} on device: {DeviceInformation.Description}with error: {ftStatus}");
            }

            SpiClockPolarity pol = SpiClockPolarity.ClockIdleLow;
            if ((_settings.Mode == SpiMode.Mode2) || (_settings.Mode == SpiMode.Mode3))
            {
                pol = SpiClockPolarity.ClockIdelHigh;
            }

            SpiClockPhase pha = SpiClockPhase.ClockLeading;
            if ((_settings.Mode == SpiMode.Mode1) || (_settings.Mode == SpiMode.Mode3))
            {
                pha = SpiClockPhase.ClockTailing;
            }

            // Configure the SPI
            ftStatus = FtFunction.FT4222_SPIMaster_Init(_ftHandle, SpiOperatingMode.Single, tfSpiDiv, pol, pha,
                (byte)_settings.ChipSelectLine);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed setup SPI on device: {DeviceInformation.Description} with error: {ftStatus}");
            }
        }

        private (FtClockRate clk, SpiClock spiClk) CalculateBestClockRate()
        {
            // Maximum is the System Clock / 1 = 80 MHz
            // Minimum is the System Clock / 512 = 24 / 256 = 93.75 KHz
            // Always take the below frequency to avoid over clocking
            if (_settings.ClockFrequency < 187500)
            {
                return (FtClockRate.Clock24MHz, SpiClock.DivideBy256);
            }

            if (_settings.ClockFrequency < 234375)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy256);
            }

            if (_settings.ClockFrequency < 312500)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy256);
            }

            if (_settings.ClockFrequency < 375000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy256);
            }

            if (_settings.ClockFrequency < 468750)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy128);
            }

            if (_settings.ClockFrequency < 625000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy128);
            }

            if (_settings.ClockFrequency < 750000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy128);
            }

            if (_settings.ClockFrequency < 937500)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy64);
            }

            if (_settings.ClockFrequency < 1250000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy64);
            }

            if (_settings.ClockFrequency < 1500000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy64);
            }

            if (_settings.ClockFrequency < 1875000)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy32);
            }

            if (_settings.ClockFrequency < 2500000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy32);
            }

            if (_settings.ClockFrequency < 3000000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy32);
            }

            if (_settings.ClockFrequency < 3750000)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy16);
            }

            if (_settings.ClockFrequency < 5000000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy16);
            }

            if (_settings.ClockFrequency < 6000000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy16);
            }

            if (_settings.ClockFrequency < 7500000)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy8);
            }

            if (_settings.ClockFrequency < 10000000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy8);
            }

            if (_settings.ClockFrequency < 12000000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy8);
            }

            if (_settings.ClockFrequency < 15000000)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy4);
            }

            if (_settings.ClockFrequency < 20000000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy4);
            }

            if (_settings.ClockFrequency < 24000000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy4);
            }

            if (_settings.ClockFrequency < 30000000)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy2);
            }

            if (_settings.ClockFrequency < 40000000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy2);
            }

            if (_settings.ClockFrequency < 48000000)
            {
                return (FtClockRate.Clock80MHz, SpiClock.DivideBy2);
            }

            if (_settings.ClockFrequency < 60000000)
            {
                return (FtClockRate.Clock48MHz, SpiClock.DivideBy1);
            }

            if (_settings.ClockFrequency < 80000000)
            {
                return (FtClockRate.Clock60MHz, SpiClock.DivideBy1);
            }

            // Anything else will be 80 MHz
            return (FtClockRate.Clock80MHz, SpiClock.DivideBy1);
        }

        /// <inheritdoc/>
        public override void Read(Span<byte> buffer)
        {
            ushort readBytes;
            var ftStatus = FtFunction.FT4222_SPIMaster_SingleRead(_ftHandle, in MemoryMarshal.GetReference(buffer),
                (ushort)buffer.Length, out readBytes, true);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(Read)} failed to read, error: {ftStatus}");
            }
        }

        /// <inheritdoc/>
        public override byte ReadByte()
        {
            Span<byte> toRead = stackalloc byte[1];
            Read(toRead);
            return toRead[0];
        }

        /// <inheritdoc/>
        public override void TransferFullDuplex(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            ushort readBytes;
            var ftStatus = FtFunction.FT4222_SPIMaster_SingleReadWrite(_ftHandle,
                in MemoryMarshal.GetReference(readBuffer), in MemoryMarshal.GetReference(writeBuffer),
                (ushort)writeBuffer.Length, out readBytes, true);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(TransferFullDuplex)} failed to do a full duplex transfer, error: {ftStatus}");
            }
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ushort bytesWritten;
            var ftStatus = FtFunction.FT4222_SPIMaster_SingleWrite(_ftHandle, in MemoryMarshal.GetReference(buffer),
                (ushort)buffer.Length, out bytesWritten, true);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(Write)} failed to write, error: {ftStatus}");
            }
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            Span<byte> toWrite = stackalloc byte[1]
            {
                value
            };
            Write(toWrite);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ftHandle.Dispose();
            base.Dispose(disposing);
        }
    }
}
