using Raspberry.Board;

namespace Raspberry
{ 
    public enum GpioPinMode
    {
        Input,
        Output,
        InputPullDown,
        InputPullUp,
        None
    }

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

        public int Number { get; private set; }
        public string Name { get; private set; }
        public bool Valid { get; private set; }
        public bool Value { get => Read(); set => Write(value); }

        public bool Read()
        {
            return Valid && controller.Read(Number) == PinValue.High;

        }
        public void Write(bool value)
        {
            if (Valid)
                controller.Write(Number, value ? PinValue.High : PinValue.Low);
        }

        public GpioPinMode Mode
        {
            get
            {
                if (Valid)
                    switch (controller.GetPinMode(Number))
                    {
                        case PinMode.Input: return GpioPinMode.Input;
                        case PinMode.Output: return GpioPinMode.Output;
                        case PinMode.InputPullDown: return GpioPinMode.InputPullDown;
                        case PinMode.InputPullUp: return GpioPinMode.InputPullUp;
                        default: return GpioPinMode.None;
                    }
                return GpioPinMode.None;
            }
            set
            {
                if (Valid)
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
    }
}
