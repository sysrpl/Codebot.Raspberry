namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// Unipole motors such as the 28BYJ-48 motor usr 512 full engine rotations to 
    /// rotate the drive shaft once. In half step mode these are 8 x 512 = 4096 steps
    /// for a full rotation. In full step modes these are 4 x 512 = 2048 steps for
    /// a full rotation.
    /// </summary>
    public static class UnipoleMode
    {
        /// <summary>Half step mode medium torque</summary>
        public const int HalfStep = 0;

        /// <summary>Full step mode (single phase) least torque</summary>
        public const int FullStepSinglePhase = 1;

        /// <summary>Full step mode (dual phase) most torque</summary>
        public const int FullStepDualPhase = 2;
    }

    /// <summary>
    /// Unipole drivers use four wires for commincation and typically are smaller
    /// using low voltage. ICs like the uln2003 package can be used to create simple
    /// driver boards for small motors such as the 28BYJ-48. These drivers boards
    /// and motors use unipole motor logic to acheive acceptable torque with slower
    /// rotational speeds.
    /// </summary>
    public class UnipoleDriver : IStepperDriver
    {
        GpioPin[] pins;
        int step;
        int mode;
        int direction;
        bool[,] modeData;

        static readonly bool[,] halfStepData = {
            { true, true, false, false, false, false, false, true },
            { false, true, true, true, false, false, false, false },
            { false, false, false, true, true, true, false, false },
            { false, false, false, false, false, true, true, true }
        };

        static readonly bool[,] singlePhaseData = {
            { true, false, false, false, true, false, false, false },
            { false, true, false, false, false, true, false, false },
            { false, false, true, false, false, false, true, false },
            { false, false, false, true, false, false, false, true }
        };

        static readonly bool[,] dualPhaseData = {
            { true, false, false, true, true, false, false, true },
            { true, true, false, false, true, true, false, false },
            { false, true, true, false, false, true, true, false },
            { false, false, true, true, false, false, true, true }
        };

        /// <summary>
        /// Initialize a unipole stepper driver using four communication pins.
        /// </summary>
        /// <param name="pin1">The Gpio pin number connected to IN1 on the driver board.</param>
        /// <param name="pin2">The Gpio pin number connected to IN2 on the driver board.</param>
        /// <param name="pin3">The Gpio pin number connected to IN3 on the driver board.</param>
        /// <param name="pin4">The Gpio pin number connected to IN4 on the driver board.</param>
        public UnipoleDriver(int pin1, int pin2, int pin3, int pin4)
        {
            pins = new GpioPin[4];
            pins[0] = Pi.Gpio.Pin(pin1, PinKind.Output);
            pins[1] = Pi.Gpio.Pin(pin2, PinKind.Output);
            pins[2] = Pi.Gpio.Pin(pin3, PinKind.Output);
            pins[3] = Pi.Gpio.Pin(pin4, PinKind.Output);
            step = 0;
            mode = 0;
            direction = 1;
            modeData = halfStepData;
        }

        public void Step()
        {
            step += direction;
            if (step < 0)
                step = 7;
            else if (step > 7)
                step = 0;
            for (var i = 0; i < pins.Length; i++)
                pins[i].Value = modeData[i, step];
        }

        public int GetSPR()
        {
            switch (mode)
            {
                case 1:
                case 2:
                    return 2048;
                default:
                    return 4096;
            }
        }

        public void SetDirection(int value)
        {
            direction = value < 0 ? -1 : 1;
        }

        public void SetMode(int value)
        {
            switch (value)
            {
                case 1:
                    modeData = singlePhaseData;
                    mode = value;
                    break;
                case 2:
                    modeData = dualPhaseData;
                    mode = value;
                    break;
                default:
                    modeData = halfStepData;
                    mode = 0;
                    break;
            }
        }

        public void SetEnable(bool value)
        {
            for (var i = 0; i < pins.Length; i++)
                pins[i].Value = value && modeData[i, step];
        }

        public void Dispose()
        {
            if (pins is null)
                return;
            SetEnable(false);
            for (var i = 0; i < pins.Length; i++)
                pins[i].Close();
            pins = null;
        }
    }
}