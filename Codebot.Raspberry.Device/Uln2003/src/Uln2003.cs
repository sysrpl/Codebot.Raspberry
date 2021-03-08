using System;
using System.Threading.Tasks;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// The 28BYJ-48 motor has 512 full engine rotations to rotate the drive shaft once.
    /// In half-step mode these are 8 x 512 = 4096 steps for a full rotation.
    /// In full-step mode these are 4 x 512 = 2048 steps for a full rotation.
    /// </summary>
    public enum StepperMode
    {
        /// <summary>Half step mode medium torque</summary>
        HalfStep,

        /// <summary>Full step mode (single phase) least torque</summary>
        FullStepSinglePhase,

        /// <summary>Full step mode (dual phase) most torque</summary>
        FullStepDualPhase
    }

    /// <summary>
    /// This class is for controlling stepper motors that are controlled by a 4 pin controller board.
    /// </summary>
    /// <remarks>It is tested and developed using the 28BYJ-48 stepper motor and the ULN2003 driver board.</remarks>
    public class Uln2003 : IDisposable
    {
        static readonly bool[,] halfStepSequence = {
            { true, true, false, false, false, false, false, true },
            { false, true, true, true, false, false, false, false },
            { false, false, false, true, true, true, false, false },
            { false, false, false, false, false, true, true, true }
        };

        static readonly bool[,] fullStepSinglePhaseSequence = {
            { true, false, false, false, true, false, false, false },
            { false, true, false, false, false, true, false, false },
            { false, false, true, false, false, false, true, false },
            { false, false, false, true, false, false, false, true }
        };

        static readonly bool[,] fullStepDualPhaseSequence = {
            { true, false, false, true, true, false, false, true },
            { true, true, false, false, true, true, false, false },
            { false, true, true, false, false, true, true, false },
            { false, false, true, true, false, false, true, true }
        };

        const int halfSteps = 4096;
        const double minDelay = 0.2d;
        const double minRPM = 1d / (halfSteps * minDelay / 60d);

        GpioPin[] pins;
        StepperMode mode;
        long engineStep;
        bool[,] currentSequence;

        /// <summary>
        /// Initialize a Uln2003 class.
        /// </summary>
        /// <param name="pin1">The Gpio pin number connected to IN1 on the driver board.</param>
        /// <param name="pin2">The Gpio pin number connected to IN2 on the driver board.</param>
        /// <param name="pin3">The Gpio pin number connected to IN3 on the driver board.</param>
        /// <param name="pin4">The Gpio pin number connected to IN4 on the driver board.</param>
        public Uln2003(int pin1, int pin2, int pin3, int pin4)
        {
            pins = new GpioPin[]
            {
                Pi.Gpio.Pin(pin1, PinKind.Output),
                Pi.Gpio.Pin(pin2, PinKind.Output),
                Pi.Gpio.Pin(pin3, PinKind.Output),
                Pi.Gpio.Pin(pin4, PinKind.Output)
            };
            SPR = halfSteps;
            RPM = 10;
            mode = StepperMode.HalfStep;
            currentSequence = halfStepSequence;
            engineStep = 0;
            stepId = 0;
            StepTask = null;
        }

        /// <summary>
        /// Gets the number of steps per 360° rotation.
        /// </summary>
        /// <remarks>This property is derived from the stepper mode.</remarks>
        public int SPR { get; private set; }

        double rpm;

        /// <summary>
        /// Gets or sets the rotational speed as calculated by 360° rotations per minute.
        /// </summary>
        public double RPM
        {
            get => rpm;
            set => rpm = value < minRPM ? minRPM : value;
        }

        /// <summary>
        /// Gets or sets the stepper's mode.
        /// </summary>
        /// <remarks>Setting this property causing the motor to stop any current motion.</remarks>
        public StepperMode Mode
        {
            get => mode;
            set
            {
                Stop();
                if (mode == value)
                    return;
                mode = value;
                switch (mode)
                {
                    case StepperMode.HalfStep:
                        currentSequence = halfStepSequence;
                        SPR = halfSteps;
                        break;
                    case StepperMode.FullStepSinglePhase:
                        currentSequence = fullStepSinglePhaseSequence;
                        SPR = halfSteps / 2;
                        break;
                    case StepperMode.FullStepDualPhase:
                        currentSequence = fullStepDualPhaseSequence;
                        SPR = halfSteps / 2;
                        break;
                }
            }
        }

        /// <summary>
        /// Get the current step task created by position or angle members.
        /// </summary>
        /// <remarks>The stop or wait methods will set this property to null.</remarks>
        public Task StepTask { get; private set; }

        long stepId;

        async Task InternalStep(long count)
        {
            const double milliseconds = 60_000d;
            Stop();
            var id = stepId;
            var target = position + count;
            await PreciseTimer.Every(milliseconds / SPR / RPM, TakeStep);

            bool TakeStep()
            {
                if (position == target)
                    return false;
                if (id != stepId)
                    return false;
                if (count > 0)
                {
                    engineStep = engineStep - 1 < 1 ? 8 : engineStep - 1;
                    position++;
                }
                else
                {
                    engineStep = engineStep + 1 > 8 ? 1 : engineStep + 1;
                    position--;
                }
                for (var i = 0; i < pins.Length; i++)
                    pins[i].Value = currentSequence[i, engineStep - 1];
                return position != target;
            }
        }

        /// <summary>
        /// Stop the motor cancelling any current step task.
        /// </summary>
        public void Stop()
        {
            stepId++;
            StepTask?.Wait();
            StepTask = null;
            foreach (var p in pins)
                p.Value = false;
        }

        /// <summary>
        /// Wait for any current step task to complete.
        /// </summary>
        public void Wait()
        {
            StepTask?.Wait();
            StepTask = null;
        }

        long position;

        /// <summary>
        /// Gets or sets the rotational step position of the motor.
        /// </summary>
        public long Position
        {
            get => position;
            set
            {
                Stop();
                var p = value - position;
                StepTask = InternalStep(p);
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle in degrees of the motor.
        /// </summary>
        public double Angle
        {
            get => (double)position / SPR * 360d;
            set
            {
                Stop();
                var a = (long)(value / 360d * SPR) - position;
                StepTask = InternalStep(a);
            }
        }

        /// <summary>
        /// Move the motor by a relative position.
        /// </summary>
        /// <param name="p">The relative steps to rotate.</param>
        /// <returns>Returns a task that can be waited or cancelled using the stop method.</returns>
        public Task MovePosition(long p)
        {
            StepTask = InternalStep(p);
            return StepTask;
        }

        /// <summary>
        /// Move the motor by a relative angle.
        /// </summary>
        /// <param name="a">The relative angle to rotate.</param>
        /// <returns>Returns a task that can be waited or cancelled using the stop method.</returns>
        public Task MoveAngle(double a)
        {
            var p = (long)(a / 360d * SPR);
            StepTask = InternalStep(p);
            return StepTask;
        }

        /// <summary>
        /// Stops the motor and resets the position and angle values to zero.
        /// </summary>
        public void Zero()
        {
            Stop();
            position = 0;
        }

        public void Dispose()
        {
            if (pins is null)
                return;
            Stop();
            foreach (var p in pins)
                p.Close();
            pins = null;
        }
    }
}