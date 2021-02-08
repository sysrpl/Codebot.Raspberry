// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Codebot.Raspberry.Board.Drivers
{
    /// <summary>
    /// A GPIO driver for the Raspberry Pi 3 or 4, running Raspbian (or, with some limitations, ubuntu)
    /// </summary>
    public class RaspberryPi3Driver : GpioDriver
    {
        private GpioDriver _internalDriver;

        /* private delegates for register Properties */
        private delegate void Set_Register(ulong value);
        private delegate ulong Get_Register();

        private readonly Set_Register _setSetRegister;
        private readonly Get_Register _getSetRegister;
        private readonly Set_Register _setClearRegister;
        private readonly Get_Register _getClearRegister;

        /// <summary>
        /// Creates an instance of the RaspberryPi3Driver.
        /// This driver works on Raspberry 3 or 4, both on Linux and on Windows
        /// </summary>
        public RaspberryPi3Driver()
        {
            _internalDriver = new RaspberryPi3LinuxDriver();
            RaspberryPi3LinuxDriver linuxDriver = _internalDriver as RaspberryPi3LinuxDriver;
            _setSetRegister = (value) => linuxDriver.SetRegister = value;
            _setClearRegister = (value) => linuxDriver.ClearRegister = value;
            _getSetRegister = () => linuxDriver.SetRegister;
            _getClearRegister = () => linuxDriver.ClearRegister;
        }

        /// <inheritdoc/>
        protected internal override int PinCount => 28;

        /// <inheritdoc/>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) => _internalDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);

        /// <inheritdoc/>
        protected internal override void ClosePin(int pinNumber) => _internalDriver.ClosePin(pinNumber);

        /// <inheritdoc/>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            switch (pinNumber)
            {
                case 3: return 2;
                case 5: return 3;
                case 7: return 4;
                case 8: return 14;
                case 10: return 15;
                case 11: return 17;
                case 12: return 18;
                case 13: return 27;
                case 15: return 22;
                case 16: return 23;
                case 18: return 24;
                case 19: return 10;
                case 21: return 9;
                case 22: return 25;
                case 23: return 11;
                case 24: return 8;
                case 26: return 7;
                case 27: return 0;
                case 28: return 1;
                case 29: return 5;
                case 31: return 6;
                case 32: return 12;
                case 33: return 13;
                case 35: return 19;
                case 36: return 16;
                case 37: return 26;
                case 38: return 20;
                case 40: return 21;
                default: throw new ArgumentException($"Board (header) pin {pinNumber} is not a GPIO pin on the {GetType().Name} device.", nameof(pinNumber));
            }
        }

        /// <inheritdoc/>
        protected internal override PinMode GetPinMode(int pinNumber) => _internalDriver.GetPinMode(pinNumber);

        /// <inheritdoc/>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) => _internalDriver.IsPinModeSupported(pinNumber, mode);

        /// <inheritdoc/>
        protected internal override void OpenPin(int pinNumber) => _internalDriver.OpenPin(pinNumber);

        /// <inheritdoc/>
        public override PinValue Read(int pinNumber) => _internalDriver.Read(pinNumber);

        /// <inheritdoc/>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) => _internalDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);

        /// <inheritdoc/>
        protected internal override void SetPinMode(int pinNumber, PinMode mode) => _internalDriver.SetPinMode(pinNumber, mode);

        /// <inheritdoc/>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => _internalDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);

        /// <inheritdoc/>
        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) => _internalDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);

        /// <inheritdoc/>
        public override void Write(int pinNumber, PinValue value) => _internalDriver.Write(pinNumber, value);

        /// <summary>
        /// Allows directly setting the "Set pin high" register. Used for special applications only
        /// </summary>
        protected ulong SetRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getSetRegister();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _setSetRegister(value);
        }

        /// <summary>
        /// Allows directly setting the "Set pin low" register. Used for special applications only
        /// </summary>
        protected ulong ClearRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _getClearRegister();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _setClearRegister(value);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _internalDriver?.Dispose();
            _internalDriver = null;
            base.Dispose(disposing);
        }
    }
}
