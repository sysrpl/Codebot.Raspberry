﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Codebot.Raspberry.Board.Drivers
{
    /// <summary>
    /// A GPIO driver for the Raspberry Pi 3 or 4, running Raspbian (or, with some limitations, ubuntu)
    /// </summary>
    internal unsafe class RaspberryPi3LinuxDriver : GpioDriver
    {
        private const int ENOENT = 2; // error indicates that an entity doesn't exist
        private const uint PeripheralBaseAddressBcm2835 = 0x2000_0000;
        private const uint PeripheralBaseAddressBcm2836 = 0x3F00_0000;
        private const uint PeripheralBaseAddressBcm2838 = 0xFE00_0000;
        private const uint PeripheralBaseAddressVideocore = 0x7E00_0000;
        private const uint InvalidPeripheralBaseAddress = 0xFFFF_FFFF;
        private const uint GpioPeripheralOffset = 0x0020_0000; // offset from the peripheral base address of the GPIO registers
        private const string GpioMemoryFilePath = "/dev/gpiomem";
        private const string MemoryFilePath = "/dev/mem";
        private const string RaspberryTreeRanges = "/proc/device-tree/soc/ranges";
        private const string ModelFilePath = "/proc/device-tree/model";

        private readonly PinState[] _pinModes;
        private RegisterView* _registerViewPointer = null;
        private static readonly object s_initializationLock = new object();

        private UnixDriver _interruptDriver = null;

        /// <summary>
        /// Returns true if this is a Raspberry Pi4
        /// </summary>
        public RaspberryPi3LinuxDriver()
        {
            _pinModes = new PinState[PinCount];
        }

        /// <summary>
        /// Raspberry Pi 3 has 28 GPIO pins.
        /// </summary>
        protected internal override int PinCount => 28;

        private bool IsPi4
        {
            get;
            set;
        }

        private void ValidatePinNumber(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber >= PinCount)
            {
                throw new ArgumentException("The specified pin number is invalid.", nameof(pinNumber));
            }
        }

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
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

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            ValidatePinNumber(pinNumber);

            _interruptDriver.OpenPin(pinNumber);
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            _interruptDriver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
        }

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void ClosePin(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            bool isOpen = _pinModes[pinNumber] != null && _pinModes[pinNumber].InUseByInterruptDriver;
            if (isOpen)
            {
                _interruptDriver.ClosePin(pinNumber);
            }

            // Set pin low and mode to input upon closing a pin
            Write(pinNumber, PinValue.Low);
            SetPinMode(pinNumber, PinMode.Input);
            _pinModes[pinNumber] = null;
        }

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                case PinMode.Output:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected internal override void OpenPin(int pinNumber)
        {
            ValidatePinNumber(pinNumber);
            Initialize();
            SetPinMode(pinNumber, PinMode.Input);
        }

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected internal unsafe override PinValue Read(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            /*
             * There are two registers that contain the value of a pin. Each hold the value of 32
             * different pins. 1 bit represents the value of a pin, 0 is PinValue.Low and 1 is PinValue.High
             */

            uint register = _registerViewPointer->GPLEV[pinNumber / 32];
            return Convert.ToBoolean((register >> (pinNumber % 32)) & 1) ? PinValue.High : PinValue.Low;
        }

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            ValidatePinNumber(pinNumber);

            _interruptDriver.OpenPin(pinNumber);
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            _interruptDriver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
        }

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            ValidatePinNumber(pinNumber);

            if (!IsPinModeSupported(pinNumber, mode))
            {
                throw new InvalidOperationException($"The pin {pinNumber} does not support the selected mode {mode}.");
            }

            /*
             * There are 6 registers(4-byte ints) that control the mode for all pins. Each
             * register controls the mode for 10 pins. Each pin uses 3 bits in the register
             * containing the mode.
             */

            // Define the shift to get the right 3 bits in the register
            int shift = (pinNumber % 10) * 3;
            // Gets a pointer to the register that holds the mode for the pin
            uint* registerPointer = &_registerViewPointer->GPFSEL[pinNumber / 10];
            uint register = *registerPointer;
            // Clear the 3 bits to 0 for the pin Number.
            register &= ~(0b111U << shift);
            // Set the 3 bits to the desired mode for that pin.
            register |= (mode == PinMode.Output ? 1u : 0u) << shift;
            *registerPointer = register;

            if (_pinModes[pinNumber] != null)
            {
                _pinModes[pinNumber].CurrentPinMode = mode;
            }
            else
            {
                _pinModes[pinNumber] = new PinState(mode);
            }

            if (mode != PinMode.Output)
            {
                SetInputPullMode(pinNumber, mode);
            }
        }

        /// <summary>
        /// Sets the resistor pull up/down mode for an input pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode of a pin to set the resistor pull up/down mode.</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void SetInputPullMode(int pinNumber, PinMode mode)
        {
            /*
             * NoOptimization is needed to force wait time to be at least minimum required cycles.
             * Also to ensure that pointer operations optimizations won't be using any locals
             * which would introduce time period where multiple threads could override value set
             * to this register.
             */
            if (IsPi4)
            {
                SetInputPullModePi4(pinNumber, mode);
                return;
            }

            byte modeToPullMode;
            switch (mode)
            {
                case PinMode.Input:
                    modeToPullMode = 0;
                    break;
                case PinMode.InputPullDown:
                    modeToPullMode = 1;
                    break;
                case PinMode.InputPullUp:
                    modeToPullMode = 2;
                    break;
                default:
                    throw new ArgumentException($"{mode} is not supported as a pull up/down mode.");
            }

            /*
             * This is the process outlined by the BCM2835 datasheet on how to set the pull mode.
             * The GPIO Pull - up/down Clock Registers control the actuation of internal pull-downs on the respective GPIO pins.
             * These registers must be used in conjunction with the GPPUD register to effect GPIO Pull-up/down changes.
             * The following sequence of events is required:
             *
             * 1. Write to GPPUD to set the required control signal (i.e.Pull-up or Pull-Down or neither to remove the current Pull-up/down)
             * 2. Wait 150 cycles – this provides the required set-up time for the control signal
             * 3. Write to GPPUDCLK0/1 to clock the control signal into the GPIO pads you wish to modify
             *    – NOTE only the pads which receive a clock will be modified, all others will retain their previous state.
             * 4. Wait 150 cycles – this provides the required hold time for the control signal
             * 5. Write to GPPUD to remove the control signal
             * 6. Write to GPPUDCLK0/1 to remove the clock
             */

            uint* gppudPointer = &_registerViewPointer->GPPUD;
            *gppudPointer &= ~0b11U;
            *gppudPointer |= modeToPullMode;

            // Wait 150 cycles – this provides the required set-up time for the control signal
            for (int i = 0; i < 150; i++)
            {
            }

            int index = pinNumber / 32;
            int shift = pinNumber % 32;
            uint* gppudclkPointer = &_registerViewPointer->GPPUDCLK[index];
            uint pinBit = 1U << shift;
            *gppudclkPointer |= pinBit;

            // Wait 150 cycles – this provides the required hold time for the control signal
            for (int i = 0; i < 150; i++)
            {
            }

            // Spec calls to reset clock after the control signal
            // Since context switch between those two instructions can potentially
            // change pull up/down value we reset the clock first.
            *gppudclkPointer &= ~pinBit;
            *gppudPointer &= ~0b11U;

            // This timeout is not documented in the spec
            // but lack of it is causing intermittent failures when
            // pull up/down is changed frequently.
            for (int i = 0; i < 150; i++)
            {
            }
        }

        /// <summary>
        /// Sets the resistor pull up/down mode for an input pin on the Raspberry Pi4.
        /// The above, complex method doesn't do anything on a Pi4 (it doesn't cause any harm, though)
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode of a pin to set the resistor pull up/down mode.</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void SetInputPullModePi4(int pinNumber, PinMode mode)
        {
            /*
             * NoOptimization is needed to force wait time to be at least minimum required cycles.
             * Also to ensure that pointer operations optimizations won't be using any locals
             * which would introduce time period where multiple threads could override value set
             * to this register.
             */
            int shift = (pinNumber & 0xf) << 1;
            uint pull = 0;
            uint bits = 0;
            switch (mode)
            {
                case PinMode.Input:
                    pull = 0;
                    break;
                case PinMode.InputPullUp:
                    pull = 1;
                    break;
                case PinMode.InputPullDown:
                    pull = 2;
                    break;
            }

            var gpioReg = _registerViewPointer;
            bits = (gpioReg->GPPUPPDN[(pinNumber >> 4)]);
            bits &= ~(3u << shift);
            bits |= (pull << shift);
            gpioReg->GPPUPPDN[(pinNumber >> 4)] = bits;
            for (int i = 0; i < 150; i++)
            {
            }
        }

        /// <summary>
        /// Blocks execution until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A structure that contains the result of the waiting operation.</returns>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            ValidatePinNumber(pinNumber);

            _interruptDriver.OpenPin(pinNumber);
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            return _interruptDriver.WaitForEvent(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Async call until an event of type eventType is received or a cancellation is requested.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="cancellationToken">The cancellation token of when the operation should stop waiting for an event.</param>
        /// <returns>A task representing the operation of getting the structure that contains the result of the waiting operation</returns>
        protected internal override ValueTask<WaitForEventResult> WaitForEventAsync(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            ValidatePinNumber(pinNumber);

            _interruptDriver.OpenPin(pinNumber);
            _pinModes[pinNumber].InUseByInterruptDriver = true;

            return _interruptDriver.WaitForEventAsync(pinNumber, eventTypes, cancellationToken);
        }

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected internal override void Write(int pinNumber, PinValue value)
        {
            ValidatePinNumber(pinNumber);

            /*
             * If the value is High, GPSET register is used. Otherwise, GPCLR will be used. For
             * both cases, a 1 is set on the corresponding bit in the register in order to set
             * the desired value.
             */

            uint* registerPointer = (value == PinValue.High) ? &_registerViewPointer->GPSET[pinNumber / 32] : &_registerViewPointer->GPCLR[pinNumber / 32];
            uint register = *registerPointer;
            register = 1U << (pinNumber % 32);
            *registerPointer = register;
        }

        protected internal ulong SetRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return *(ulong*)(_registerViewPointer->GPSET); }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { *(ulong*)(_registerViewPointer->GPSET) = value; }
        }

        protected internal ulong ClearRegister
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return *(ulong*)(_registerViewPointer->GPCLR); }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { *(ulong*)(_registerViewPointer->GPCLR) = value; }
        }

        private static uint ReadUint(byte[] data)
        {
            // return BinaryPrimitives.ReadUInt32BigEndian(data);
            // return BitConverter.ToUInt32(data, 0);
            int i = 0;
            byte tmp = data[i + 3];
            data[i + 3] = data[i];
            data[i] = tmp;
            tmp = data[i + 2];
            data[i + 2] = data[i + 1];
            data[i + 1] = tmp;
            return BitConverter.ToUInt32(data, i);
        }

        /// <summary>
        /// Returns the peripheral base address on the CPU bus of the raspberry pi based on the ranges set within the device tree.
        /// </summary>
        /// <remarks>
        /// The range examined in this method is essentially a mapping between where the peripheral base address on the videocore bus and its
        /// address on the cpu bus. The return value is 32bit (is in the first 4GB) even on 64 bit operating systems (debian / ubuntu tested) but may change in the future
        /// This method is based on bcm_host_get_peripheral_address() in libbcm_host which may not exist in all linux distributions.
        /// </remarks>
        /// <returns>This returns the peripheral base address as a 32 bit address or 0xFFFFFFFF when in error.</returns>
        private uint GetPeripheralBaseAddress()
        {
            uint cpuBusPeripheralBaseAddress = InvalidPeripheralBaseAddress;
            uint vcBusPeripheralBaseAddress;

            using (BinaryReader rdr = new BinaryReader(File.Open(RaspberryTreeRanges, FileMode.Open, FileAccess.Read)))
            {
                // get the Peripheral Base Address on the VC bus from the device tree this is to be used to verify that
                // the right thing is being read and should always be 0x7E000000
                vcBusPeripheralBaseAddress = ReadUint(rdr.ReadBytes(4));  //BinaryPrimitives.ReadUInt32BigEndian(rdr.ReadBytes(4));

                // get the Peripheral Base Address on the CPU bus from the device tree.
                cpuBusPeripheralBaseAddress = ReadUint(rdr.ReadBytes(4));  // BinaryPrimitives.ReadUInt32BigEndian(rdr.ReadBytes(4));

                // if the CPU bus Peripheral Base Address is 0 then assume that this is a 64 bit address and so read the next 32 bits.
                if (cpuBusPeripheralBaseAddress == 0)
                {
                    cpuBusPeripheralBaseAddress = ReadUint(rdr.ReadBytes(4)); ;  //BinaryPrimitives.ReadUInt32BigEndian(rdr.ReadBytes(4));
                }

                // if the address values don't fall withing known values for the chipsets associated with the Pi2, Pi3 and Pi4 then assume an error
                // These addresses are coded into the device tree and the dts source for the device tree is within https://github.com/raspberrypi/linux/tree/rpi-4.19.y/arch/arm/boot/dts
                if (vcBusPeripheralBaseAddress != PeripheralBaseAddressVideocore || !(cpuBusPeripheralBaseAddress == PeripheralBaseAddressBcm2835 || cpuBusPeripheralBaseAddress == PeripheralBaseAddressBcm2836 || cpuBusPeripheralBaseAddress == PeripheralBaseAddressBcm2838))
                {
                    cpuBusPeripheralBaseAddress = InvalidPeripheralBaseAddress;
                }
            }

            return cpuBusPeripheralBaseAddress;
        }

        private void InitializeInterruptDriver()
        {
            try
            {
                _interruptDriver = new LibGpiodDriver(0);
            }
            catch (PlatformNotSupportedException)
            {
                _interruptDriver = new InterruptSysFsDriver(this);
            }
        }

        private void Initialize()
        {
            uint gpioRegisterOffset = 0;
            int fileDescriptor;
            int win32Error;

            if (_registerViewPointer != null)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (_registerViewPointer != null)
                {
                    return;
                }

                // try and open /dev/gpiomem
                fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    win32Error = Marshal.GetLastWin32Error();

                    // if the failure is NOT because /dev/gpiomem doesn't exist then throw an exception at this point.
                    // if it were anything else then it is probably best not to try and use /dev/mem on the basis that
                    // it would be better to solve the issue rather than use a method that requires root privileges
                    if (win32Error != ENOENT)
                    {
                        throw new IOException($"Error {win32Error} initializing the Gpio driver.");
                    }

                    // if /dev/gpiomem doesn't seem to be available then let's try /dev/mem
                    fileDescriptor = Interop.open(MemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                    if (fileDescriptor == -1)
                    {
                        throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                    }
                    else // success so set the offset into memory of the gpio registers
                    {
                        gpioRegisterOffset = InvalidPeripheralBaseAddress;

                        try
                        {
                            // get the periphal base address from the libbcm_host library which is the reccomended way
                            // according to the RasperryPi website
                            gpioRegisterOffset = Interop.libbcmhost.bcm_host_get_peripheral_address();

                            // if we get zero back then we use our own internal method. This can happen
                            // on a Pi4 if the userland libraries haven't been updated and was fixed in Jul/Aug 2019.
                            if (gpioRegisterOffset == 0)
                            {
                                gpioRegisterOffset = GetPeripheralBaseAddress();
                            }
                        }
                        catch (DllNotFoundException)
                        {
                            // if the code gets here then then use our internal method as libbcm_host isn't available.
                            gpioRegisterOffset = GetPeripheralBaseAddress();
                        }

                        if (gpioRegisterOffset == InvalidPeripheralBaseAddress)
                        {
                            throw new InvalidOperationException("Error - Unable to determine peripheral base address.");
                        }

                        // add on the offset from the peripheral base address to point to the gpio registers
                        gpioRegisterOffset += GpioPeripheralOffset;
                    }
                }

                IntPtr mapPointer = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize, (MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE), MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)gpioRegisterOffset);
                if (mapPointer.ToInt64() == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                Interop.close(fileDescriptor);
                _registerViewPointer = (RegisterView*)mapPointer;

                // Detect whether we're running on a Raspberry Pi 4
                IsPi4 = false;
                try
                {
                    if (File.Exists(ModelFilePath))
                    {
                        string model = File.ReadAllText(ModelFilePath, System.Text.Encoding.ASCII);
                        if (model.Contains("Raspberry Pi 4"))
                        {
                            IsPi4 = true;
                        }
                    }
                }
                catch (Exception x)
                {
                    // This should not normally fail, but we currently don't know how this behaves on different operating systems. Therefore, we ignore
                    // any exceptions in release and just continue as Pi3 if something fails.
                    // If in debug mode, we might want to check what happened here (i.e unsupported OS, incorrect permissions)
                    Debug.Fail($"Unexpected exception: {x}");
                }

                InitializeInterruptDriver();
            }
        }

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            ValidatePinNumber(pinNumber);

            var entry = _pinModes[pinNumber];
            if (entry == null)
            {
                throw new InvalidOperationException("Can not get a pin mode of a pin that is not open.");
            }

            return entry.CurrentPinMode;
        }

        protected override void Dispose(bool disposing)
        {
            if (_registerViewPointer != null)
            {
                Interop.munmap((IntPtr)_registerViewPointer, 0);
                _registerViewPointer = null;
            }

            if (_interruptDriver != null)
            {
                _interruptDriver.Dispose();
                _interruptDriver = null;
            }
        }

        private class PinState
        {
            public PinState(PinMode currentMode)
            {
                CurrentPinMode = currentMode;
                InUseByInterruptDriver = false;
            }

            public PinMode CurrentPinMode
            {
                get;
                set;
            }

            public bool InUseByInterruptDriver
            {
                get;
                set;
            }
        }
    }
}
