using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codebot.Raspberry.Board;
using Codebot.Raspberry.Common;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The InvalidGpioPinException exception is throw when a operation is attempted 
    /// and the pin does not represent a valid logical Gpio pin.
    /// </summary>
    public class InvalidGpioPinException : Exception { }

    /// <summary>
    /// The InvalidGpioKindException exception is thrown if a read or write
    /// attempt is made when a pin is not in an input or output kind respectfully.
    /// </summary>
    public class InvalidGpioKindException : Exception { }

    /// <summary>
    /// The Gpio pin class allows for reading and writing to Gpio pin
    /// Gpio use requires these packages to be installed: gpiod libgpiod-dev 
    /// </summary>
    /// <remarks>Use Pi.Gpio to gain access to a Pin</remarks>
    public class GpioPin : IDisposable
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
        /// Close this pin rendering it invalid.
        /// </summary>
        public void Close()
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            Pi.Gpio.Close(Number);
            Number = 0;
            Name = string.Empty;
            Valid = false;
        }

        /// <summary>
        /// The time in millisecond to assume under which limit an input event 
        /// was a bounce rather than a legitimate voltage rise or fall.
        /// </summary>
        public double BounceDelay { get; set; } = 30;

        /// <summary>
        /// The logical (Gpio) number of this pin.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// The name of the pin.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Valid is true if the pin has a name.
        /// </summary>
        public bool Valid { get; private set; }

        /// <summary>
        /// Value reads or write from the pin.
        /// </summary>
        public bool Value { get => Read(); set => Write(value); }

        /// <summary>
        /// When kind is set to input, read and wait methods can be used.
        /// </summary>
        /// <value>The kind to set</value>
        /// <remarks>If the logical pin is invalid then kind will always be None</remarks>
        public PinKind Kind
        {
            get
            {
                if (!Valid)
                    return PinKind.None;
                switch (controller.GetPinMode(Number))
                {
                    case PinMode.Input: return PinKind.Input;
                    case PinMode.Output: return PinKind.Output;
                    case PinMode.InputPullDown: return PinKind.InputPullDown;
                    case PinMode.InputPullUp: return PinKind.InputPullUp;
                    default: return PinKind.None;
                }
            }
            set
            {
                if (value == Kind)
                    return;
                if (!Valid)
                    return;
                switch (value)
                {
                    case PinKind.Input:
                        controller.SetPinMode(Number, PinMode.Input);
                        break;
                    case PinKind.Output:
                        controller.SetPinMode(Number, PinMode.Output);
                        break;
                    case PinKind.InputPullDown:
                        controller.SetPinMode(Number, PinMode.InputPullDown);
                        break;
                    case PinKind.InputPullUp:
                        controller.SetPinMode(Number, PinMode.InputPullUp);
                        break;
                }
            }
        }

        /// <summary>
        /// IsInput is true if pin kind is a type of of input.
        /// </summary>
        private bool IsInput
        {
            get
            {
                switch (Kind)
                {
                    case PinKind.None:
                    case PinKind.Output:
                        return false;
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// IsOutput is true if pin kind is a set to output.
        /// </summary>
        private bool IsOutput
        {
            get
            {
                return Kind == PinKind.Output;
            }
        }

        /// <summary>
        /// Called by WaitLow or WaitHigh.
        /// </summary>
        private bool WaitRead(double timeout, bool target, out double elapsed)
        {
            var timer = new PreciseTimer();
            elapsed = double.NaN;
            if (Read() == target)
            {
                elapsed = timer.ElapsedMilliseconds;
                return true;
            }
            var result = controller.WaitForEvent(Number,
                target ? PinEventTypes.Rising : PinEventTypes.Falling,
                TimeSpan.FromMilliseconds(timeout));
            var time = timer.ElapsedMilliseconds;
            if (result.TimedOut || (time > timeout))
                return false;
            elapsed = time;
            return true;
        }

        /// <summary>
        /// Wait for a low value to be read on the pin and optionally output
        /// the elapsed time.
        /// </summary>
        /// <returns>Returns <c>true</c> if the pin went low before timeout has expired</returns>
        /// <param name="timeout">Maximum time in milliseconds to wait</param>
        /// <param name="elapsed">Optional elapsed time in milliseconds until pin was read as low, or NaN if timeout was reached first</param>
        /// <remarks>Kind must be set to Input in order to use WaitLow</remarks>
        public bool WaitLow(double timeout, out double elapsed)
        {
            return WaitRead(timeout, false, out elapsed);
        }
        public bool WaitLow(double timeout)
        {
            return WaitRead(timeout, false, out double elapsed);
        }

        /// <summary>
        /// Wait indefinately for the pin to read low.
        /// </summary>
        public void WaitLow()
        {
            if (!Read())
                return;
            WaitForEdge(PinEdge.Falling);
        }

        /// <summary>
        /// Wait for a high value to be read on the pin and optionally output
        /// the elapsed time.
        /// </summary>
        /// <returns>Returns <c>true</c> if the pin went high before timeout has expired</returns>
        /// <param name="timeout">Maximum time in milliseconds to wait</param>
        /// <param name="elapsed">Optional elapsed time in milliseconds until pin was read as high, or NaN if timeout was reached first</param>
        /// <remarks>Kind must be set to Input in order to use WaitHigh</remarks>
        public bool WaitHigh(double timeout, out double elapsed)
        {
            return WaitRead(timeout, true, out elapsed);
        }
        public bool WaitHigh(double timeout)
        {
            return WaitRead(timeout, true, out double elapsed);
        }

        /// <summary>
        /// Wait indefinately for the pin to read high.
        /// </summary>
        public void WaitHigh()
        {
            if (Read())
                return;
            WaitForEdge(PinEdge.Rising);
        }

        /// <summary>
        /// Waits timeout milliseconds for a rising or falling edge and optionally 
        /// output the elapsed time.
        /// </summary>
        /// <returns>Return <c>true</c> if the edge was triggered before timeout milliseconds.</returns>
        /// <param name="edge">The edge to trigger completion of the wait.</param>
        /// <param name="timeout">The timeout in milliseconds after which <c>false</c> is returned.</param>
        /// <param name="elapsed">Optional elapsed time in milliseconds until edge was triggered, or NaN if timeout was reached first</param>
        public bool WaitForEdge(PinEdge edge, double timeout, out double elapsed)
        {
            var timer = new PreciseTimer();
            elapsed = double.NaN;
            if (WaitForEdge(edge, timeout))
            {
                var time = timer.ElapsedMilliseconds;
                if (time < timeout)
                {
                    elapsed = time;
                    return true;
                }
            }
            return false;
        }
        public bool WaitForEdge(PinEdge edge, double timeout)
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsInput)
                throw new InvalidGpioKindException();
            var result = controller.WaitForEvent(Number,
                edge == PinEdge.Rising ? PinEventTypes.Rising : PinEventTypes.Falling,
                TimeSpan.FromMilliseconds(timeout));
            return !result.TimedOut;
        }

        /// <summary>
        /// Waits indefinately for a rising or falling edge.
        /// </summary>
        public void WaitForEdge(PinEdge edge)
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsInput)
                throw new InvalidGpioKindException();
            var token = new CancellationToken();
            var result = controller.WaitForEvent(Number,
                edge == PinEdge.Rising ? PinEventTypes.Rising : PinEventTypes.Falling,
                token);
        }

        /// <summary>
        /// Waits asynchronously timeout milliseconds for a rising or falling edge.
        /// </summary>
        /// <returns>Return an asynchronously task.</returns>
        /// <param name="edge">The edge to trigger a the wait to end.</param>
        /// <param name="timeout">The timeout in milliseconds after which the task expires.</param>
        public async Task<bool> WaitForEdgeAsync(PinEdge edge, double timeout)
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsInput)
                throw new InvalidGpioKindException();
            var task = await controller.WaitForEventAsync(Number,
                edge == PinEdge.Rising ? PinEventTypes.Rising : PinEventTypes.Falling,
                TimeSpan.FromMilliseconds(timeout)).AsTask();
            return !task.TimedOut;
        }

        /// <summary>
        /// Read the state of the pin
        /// </summary>
        /// <returns>Returns true if the pin was high or false if it was low</returns>
        /// <remarks>Kind must be set to an input in order to use Read</remarks>
        public bool Read()
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsInput)
                throw new InvalidGpioKindException();
            return controller.Read(Number) == PinValue.High;
        }

        /// <summary>
        /// Read the state of the pin quickly without safety checks
        /// </summary>
        /// <returns>Returns true if the pin was high or false if it was low</returns>
        /// <remarks>Kind must be set to an input in order to use Read</remarks>
        public bool ReadFast()
        {
            return controller._driver.Read(Number) == PinValue.High;
        }

        /// <summary>
        /// Write the state of the pin
        /// </summary>
        /// <param name="value">When value of <c>true</c> pin to set high otherwise the pin is set to low</param>
        /// <remarks>Kind must be set to output in order to use Write</remarks>
        public void Write(bool value)
        {
            if (!Valid)
                throw new InvalidGpioPinException();
            if (!IsOutput)
                throw new InvalidGpioKindException();
            controller.Write(Number, value ? PinValue.High : PinValue.Low);
        }

        /// <summary>
        /// Write the state of the pin quickly without safety checks
        /// </summary>
        /// <param name="value">When value of <c>true</c> pin to set high otherwise the pin is set to low</param>
        /// <remarks>Kind must be set to output in order to use Write</remarks>
        public void WriteFast(bool value)
        {
            controller._driver.Write(Number, value ? PinValue.High : PinValue.Low);
        }

        private class EdgeSink
        {
            readonly GpioPin pin;
            readonly PinEdge edge;
            bool connected;
            double now;

            public EdgeSink(GpioPin p, PinEdge e)
            {
                pin = p;
                edge = e;
                connected = false;
                now = 0;
                Handler = null;
            }

            public EventHandler<PinEventHandlerArgs> Handler { get; private set; }

            void VoltageChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
            {
                var n = Pi.Now;
                var b = true;
                if (n - now > pin.BounceDelay)
                {
                    now = n;
                    b = false;
                }
                Handler(pin, new PinEventHandlerArgs(pin, edge, b));
            }

            public void Connect(EventHandler<PinEventHandlerArgs> h)
            {
                if (connected)
                    return;
                Handler = h;
                connected = true;
                pin.controller.RegisterCallbackForPinValueChangedEvent(pin.Number, 
                    edge == PinEdge.Rising ? PinEventTypes.Rising : PinEventTypes.Falling,
                    VoltageChanged);
            }

            public void Disconnect()
            {
                if (!connected)
                    return;
                Handler = null;
                connected = false;
                pin.controller.UnregisterCallbackForPinValueChangedEvent(pin.Number, 
                    VoltageChanged);
            }
        }

        List<EdgeSink> risingSinks;

        /// <summary>
        /// The OnRisingEdge can be used to trigger an event handler when a pin 
        /// is an input kind and the voltage rises above ground.
        /// </summary>
        public event EventHandler<PinEventHandlerArgs> OnRisingEdge
        { 
            add 
            {
                if (risingSinks is null)
                    risingSinks = new List<EdgeSink>();
                var sink = new EdgeSink(this, PinEdge.Rising);
                sink.Connect(value);
                risingSinks.Add(sink);
            }
            remove
            {
                var sink = risingSinks.First(s => s.Handler == value);
                risingSinks.Remove(sink);
                sink.Disconnect();
            }
        }

        List<EdgeSink> fallingSinks;

        /// <summary>
        /// The OnFallingEdge can be used to trigger an event handler when a pin 
        /// is an input kind and the voltage falls to ground.
        /// </summary>
        public event EventHandler<PinEventHandlerArgs> OnFallingEdge
        {
            add
            {
                if (fallingSinks is null)
                    fallingSinks = new List<EdgeSink>();
                var sink = new EdgeSink(this, PinEdge.Rising);
                sink.Connect(value);
                fallingSinks.Add(sink);
            }
            remove
            {
                var sink = fallingSinks.First(s => s.Handler == value);
                fallingSinks.Remove(sink);
                sink.Disconnect();
            }
        }

        /// <summary>
        /// Remove any connected rising or falling event handlers.
        /// </summary>
        public void RemoveEvents()
        {
            if (!(risingSinks is null))
                foreach (var s in risingSinks)
                    s.Disconnect();
            risingSinks = null;
            if (!(fallingSinks is null))
                foreach (var s in fallingSinks)
                    s.Disconnect();
            fallingSinks = null;
        }

        /// <summary>
        /// Dispose releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (Valid)
                Close();
        }
    }
}
