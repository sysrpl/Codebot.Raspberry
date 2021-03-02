using System;
using System.Diagnostics;
using Codebot.Raspberry;

namespace Tests
{
    /// <summary>
    /// The 28BYJ-48 motor has 512 full engine rotations to rotate the drive shaft once.
    /// In half-step mode these are 8 x 512 = 4096 steps for a full rotation.
    /// In full-step mode these are 4 x 512 = 2048 steps for a full rotation.
    /// </summary>
    public enum StepperMode
    {
        /// <summary>Half step mode</summary>
        HalfStep,

        /// <summary>Full step mode (single phase)</summary>
        FullStepSinglePhase,

        /// <summary>Full step mode (dual phase)</summary>
        FullStepDualPhase
    }

    /// <summary>
    /// This class is for controlling stepper motors that are controlled by a 4 pin controller board.
    /// </summary>
    /// <remarks>It is tested and developed using the 28BYJ-48 stepper motor and the ULN2003 driver board.</remarks>
    public class Uln2003 : IDisposable
    {
        /// <summary>
        /// Default delay in microseconds.
        /// </summary>
        const long defaultDelay = 1000;

        static readonly bool[,] halfStepSequence =
        {
            { true, true, false, false, false, false, false, true },
            { false, true, true, true, false, false, false, false },
            { false, false, false, true, true, true, false, false },
            { false, false, false, false, false, true, true, true }
        };

        static readonly bool[,] fullStepSinglePhaseSequence =
        {
            { true, false, false, false, true, false, false, false },
            { false, true, false, false, false, true, false, false },
            { false, false, true, false, false, false, true, false },
            { false, false, false, true, false, false, false, true }
        };

        static readonly bool[,] fullStepDualPhaseSequence = 
        {
            { true, false, false, true, true, false, false, true },
            { true, true, false, false, true, true, false, false },
            { false, true, true, false, false, true, true, false },
            { false, false, true, true, false, false, true, true }
        };

        readonly int stepsToRotate;
        int stepsToRotateInMode;
        GpioPin[] pins;
        StepperMode mode;
        bool[,] currentSequence;
        Stopwatch stopwatch;

        /// <summary>
        /// Initialize a Uln2003 class.
        /// </summary>
        /// <param name="pin1">The GPIO pin number which corresponds pin A on ULN2003 driver board.</param>
        /// <param name="pin2">The GPIO pin number which corresponds pin B on ULN2003 driver board.</param>
        /// <param name="pin3">The GPIO pin number which corresponds pin C on ULN2003 driver board.</param>
        /// <param name="pin4">The GPIO pin number which corresponds pin D on ULN2003 driver board.</param>
        /// <param name="halfModeSteps">Amount of steps needed to rotate motor once in HalfStepMode.</param>
        public Uln2003(int pin1, int pin2, int pin3, int pin4, int halfModeSteps = 4096)
        {
            currentSequence = halfStepSequence;
            stopwatch = new Stopwatch();
            pins = new GpioPin[4];
            pins[0] = Pi.Gpio.Pin(pin1, PinKind.Output);
            pins[1] = Pi.Gpio.Pin(pin2, PinKind.Output);
            pins[2] = Pi.Gpio.Pin(pin3, PinKind.Output);
            pins[3] = Pi.Gpio.Pin(pin4, PinKind.Output);
            stepsToRotate = halfModeSteps;
            Mode = StepperMode.HalfStep;
        }

        /// <summary>
        /// Sets the motor speed to revolutions per minute.
        /// </summary>
        /// <remarks>Default revolutions per minute for 28BYJ-48 is approximately 15.</remarks>
        public short RPM { get; set; }

        /// <summary>
        /// Sets the stepper's mode.
        /// </summary>
        public StepperMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                switch (mode)
                {
                    case StepperMode.HalfStep:
                        currentSequence = halfStepSequence;
                        stepsToRotateInMode = stepsToRotate;
                        break;
                    case StepperMode.FullStepSinglePhase:
                        currentSequence = fullStepSinglePhaseSequence;
                        stepsToRotateInMode = stepsToRotate / 2;
                        break;
                    case StepperMode.FullStepDualPhase:
                        currentSequence = fullStepDualPhaseSequence;
                        stepsToRotateInMode = stepsToRotate / 2;
                        break;
                }
            }
        }

        /// <summary>
        /// Stop the motor.
        /// </summary>
        public void Stop()
        {
            stopwatch.Stop();
            foreach (var p in pins)
                p.Write(false);
        }

        /// <summary>
        /// Moves the motor count steps. If count is negative, the motor moves in the reverse direction.
        /// </summary>
        public void Step(long count)
        {
            var lastStepTime = 0d;
            stopwatch.Restart();
            long stepMicrosecondsDelay = RPM > 0 ? 60 * 1000 * 1000 / stepsToRotateInMode / RPM : defaultDelay;
            var clockwise = count >= 0;
            count = Math.Abs(count);
            long engineStep = 0;
            long step = 0;
            while (step < count)
            {
                var elapsedMicroseconds = stopwatch.Elapsed.TotalMilliseconds * 1000d;
                if (elapsedMicroseconds - lastStepTime >= stepMicrosecondsDelay)
                {
                    lastStepTime = elapsedMicroseconds;
                    if (clockwise)
                        engineStep = engineStep - 1 < 1 ? 8 : engineStep - 1;
                    else
                        engineStep = engineStep + 1 > 8 ? 1 : engineStep + 1;
                    for (var i = 0; i < pins.Length; i++)
                        pins[i].Write(currentSequence[i, engineStep - 1]);
                    step++;
                }
            }
        }

        /// <summary>
        /// Rotates the motor by a given angle.
        /// </summary>
        /// <param name="angle">Degrees to rotate the motor.</param>
        public void Rotate(double angle)
        {
            Step((long)Math.Round(angle / 360d * stepsToRotateInMode));
        }

        public void Dispose()
        {
            if (pins.Length > 0)
                Stop();
            foreach (var p in pins)
                p.Dispose();
            pins = new GpioPin[0];
        }
    }
}