using System;
using Codebot.Raspberry.Board;
using Codebot.Raspberry.Common;

namespace Codebot.Raspberry
{
    public class InvalidGpioPinException : Exception { }
    public class InvalidGpioModeException : Exception { }

    /// <summary>
    /// Gpio pin mode determines if a pin can read or write
    /// </summary>
    public enum GpioPinMode
    {
        Input,
        Output,
        InputPullDown,
        InputPullUp,
        None
    }

    /// <summary>
    /// The Gpio pin class allows for reading and writing to Gpio pin
    /// Gpio use requires these packages to be installed: gpiod libgpiod-dev 
    /// </summary>
    /// <remarks>Use Pi.Gpio to gain access to a GpioPin</remarks>
    public class GpioPin
    {
        private GpioController controller;

        internal GpioPin(GpioController controller, int number)
        {
            this.controller = controller;
            Number = number;
            Name = Pi.Gpio.Name(number);
            Valid = Name != string.Empty;
        }

        /// <summary>
        /// The logical (Gpio) number of this pin
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// The name of the pin
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Valid is true if the pin has a name
        /// </summary>
        public bool Valid { get; private set; }


        /// <summary>
        /// Value reads or write from the pin
        /// </summary>
        public bool Value { get => Read(); set => Write(value); }

        /// <summary>
        /// When Mode is set to input, read and wait can be used
        /// </summary>
        /// <value>The mode to set</value>
        /// <remarks>If the logical pin is invalid mode will always be None</remarks>
        public GpioPinMode Mode
        {
            get
            {
                if (!Valid)
                    return GpioPinMode.None;
                switch (controller.GetPinMode(Number))
                {
                    case PinMode.Input: return GpioPinMode.Input;
                    case PinMode.Output: return GpioPinMode.Output;
                    case PinMode.InputPullDown: return GpioPinMode.InputPullDown;
                    case PinMode.InputPullUp: return GpioPinMode.InputPullUp;
                    default: return GpioPinMode.None;
                }
            }
            set
            {
                if (!Valid)
                    return;
                switch (value)
                {
                    case GpioPinMode.Input:
                        controller.SetPinMode(Number, PinMode.Input);
                        break;
                    case GpioPinMode.Output:
                        controller.SetPinMode(Number, PinMode.Output);
                        break;
                    case GpioPinMode.InputPullDown:
                        controller.SetPinMode(Number, PinMode.InputPullDown);
                        break;
                    case GpioPinMode.InputPullUp:
                        controller.SetPinMode(Number, PinMode.InputPullUp);
                        break;
                }
            }
        }


        private bool IsInput
        {
            get
            {
                switch (Mode)
                {
                    case GpioPinMode.None:
                    case GpioPinMode.Output:
                        return false;
                    default:
                        return true;
                }
            }
        }

        private bool IsOutput
        {
            get
            {
                return Mode == GpioPinMode.Output;
            }
        }

        private bool WaitRead(double timeout, PinValue pinValue, out double elapsed)
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsInput)
                throw new InvalidGpioModeException();
            elapsed = double.NaN;
            var target = pinValue == PinValue.High;
            var timer = new Timer();
            while (true)
            {
                if (controller._driver.Read(Number) == target)
                {
                    elapsed = timer.ElapsedMilliseconds;
                    return true;
                }
                if (timer.ElapsedMilliseconds > timeout)
                    return false;
            }
        }

        /// <summary>
        /// Wait for a low value to be read on the pin and output the elapsed time
        /// </summary>
        /// <returns>Returns <c>true</c> if the pin went low before timeout has expired</returns>
        /// <param name="timeout">Maximum time in milliseconds to wait</param>
        /// <param name="elapsed">Elapsed time in milliseconds until pin was read as low, or NaN if timeout was reached first</param>
        /// <remarks>Mode must be set to Input in order to use WaitLow</remarks>
        public bool WaitLow(double timeout, out double elapsed)
        {
            return WaitRead(timeout, false, out elapsed);
        }
        public bool WaitLow(double timeout)
        {
            return WaitRead(timeout, false, out double elapsed);
        }

        /// <summary>
        /// Wait for a high value to be read on the pin and output the elapsed time
        /// </summary>
        /// <returns>Returns <c>true</c> if the pin went high before timeout has expired</returns>
        /// <param name="timeout">Maximum time in milliseconds to wait</param>
        /// <param name="elapsed">Elapsed time in milliseconds until pin was read as high, or NaN if timeout was reached first</param>
        /// <remarks>Mode must be set to Input in order to use WaitHigh</remarks>
        public bool WaitHigh(double timeout, out double elapsed)
        {
            return WaitRead(timeout, PinValue.High, out elapsed);
        }
        public bool WaitHigh(double timeout)
        {

            return WaitRead(timeout, PinValue.High, out double elapsed);
        }

        /// <summary>
        /// Read the state of the pin
        /// </summary>
        /// <returns>Returns true if the pin was high or false if it was low</returns>
        /// <remarks>Mode must be set to an input in order to use Read</remarks>
        public bool Read()
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsInput)
                throw new InvalidGpioModeException();
            return controller.Read(Number) == PinValue.High;
        }

        /// <summary>
        /// Read the state of the pin quickly without safety checks
        /// </summary>
        /// <returns>Returns true if the pin was high or false if it was low</returns>
        /// <remarks>Mode must be set to an input in order to use Read</remarks>
        public bool ReadFast()
        {
            return controller._driver.Read(Number) == PinValue.High;
        }

        /// <summary>
        /// Write the state of the pin
        /// </summary>
        /// <param name="value">When value of <c>true</c> pin to set high otherwise the pin is set to low</param>
        /// <remarks>Mode must be set to output in order to use Write</remarks>
        public void Write(bool value)
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsOutput)
                throw new InvalidGpioModeException();
            controller.Write(Number, value ? PinValue.High : PinValue.Low);
        }

        /// <summary>
        /// Write the state of the pin quickly without safety checks
        /// </summary>
        /// <param name="value">When value of <c>true</c> pin to set high otherwise the pin is set to low</param>
        /// <remarks>Mode must be set to output in order to use Write</remarks>
        public void WriteFast(bool value)
        {
            controller._driver.Write(Number, value ? PinValue.High : PinValue.Low);
        }
    }
}
