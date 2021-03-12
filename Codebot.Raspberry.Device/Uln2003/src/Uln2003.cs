using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

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
    /// The stepper move enumeration is used to set the motor angle or position 
    /// in absolute or relative unts.
    /// </summary>
    public enum StepperMove
    {
        /// <summary>Use absolute units measured from the zero position.</summary>
        Absolute,
        /// <summary>Use relative units measured from the current position.</summary>
        Relative
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
            nextSteps = new List<StepItem>();
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
        /// Get the current step task created by step or angle members.
        /// </summary>
        /// <remarks>The stop or wait methods will set this property to null.</remarks>
        public Task StepTask { get; private set; }

        struct StepItem
        {
            public long Count;
            public StepperMove Move;
            public double Delay;
            public double RPM;
        }

        List<StepItem> nextSteps;
        long stepId;

        async Task InternalStepAsync(long count)
        {
            const double milliseconds = 60_000d;
            var id = stepId;
            var target = step + count;
            await PreciseTimer.Every(milliseconds / SPR / RPM, TakeStep);

            bool TakeStep()
            {
                if (id != stepId)
                    return false;
                if (step == target)
                {
                    ContinueStep(id);
                    return false;
                }
                if (count > 0)
                {
                    engineStep = engineStep + 1 > 8 ? 1 : engineStep + 1;
                    step++;
                }
                else
                {
                    engineStep = engineStep - 1 < 1 ? 8 : engineStep - 1;
                    step--;
                }
                for (var i = 0; i < pins.Length; i++)
                    pins[i].Value = currentSequence[i, engineStep - 1];
                return true;
            }
        }

        void ContinueStep(long id)
        {
            if (id == stepId && !(OnStepTaskComplete is null))
                OnStepTaskComplete(this, EventArgs.Empty);
            lock (this)
            {
                if (id != stepId)
                {
                    nextSteps.Clear();
                    return;
                }
                if (nextSteps.Count < 1)
                    return;
                var item = nextSteps[0];
                nextSteps.RemoveAt(0);
                if (item.Delay > 0)
                    PreciseTimer.Wait(item.Delay);
                if (id != stepId)
                {
                    nextSteps.Clear();
                    return;
                }
                if (item.RPM > 0)
                    RPM = item.RPM;
                stepId++;
                var count = item.Move == StepperMove.Relative ? item.Count : item.Count - step;
                StepTask = InternalStepAsync(count);
            }
        }

        void FirstStep(long count)
        {
            Stop();
            StepTask = InternalStepAsync(count);
        }

        void NextStep(long count, StepperMove move, double delay, double rpm)
        {
            lock (this)
            {
                var item = new StepItem()
                {
                    Count = count,
                    Move = move,
                    Delay = delay,
                    RPM = rpm
                };
                nextSteps.Add(item);
                if (!IsStepping)
                    StepTask = InternalStepAsync(0);
            }
        }

        /// <summary>
        /// Stop the motor cancelling any current or pending step tasks.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                stepId++;
                StepTask?.Wait();
                StepTask = null;
                foreach (var p in pins)
                    p.Value = false;
                nextSteps.Clear();
            }
        }

        /// <summary>
        /// IsStepping is true if the motor is processing a step task.
        /// </summary>
        public bool IsStepping
        {
            get => !(StepTask is null) && !StepTask.IsCompleted;
        }

        /// <summary>
        /// Wait for any current or pending step tasks to complete.
        /// <param name="milliseconds">Optionally add some milliseconds to the 
        /// end of the wait time.</param>
        /// </summary>
        public void Wait(double milliseconds = 0)
        {
            StepTask?.Wait();
            StepTask = null;
            var i = 0;
            while (true)
            {
                lock (this)
                    i = nextSteps.Count;
                if (i == 0)
                    break;
                PreciseTimer.Wait(1);
            }
            StepTask?.Wait();
            StepTask = null;
            if (milliseconds > 0)
                PreciseTimer.Wait(milliseconds);
        }

        long step;

        /// <summary>
        /// Gets or sets absolute position of the motor.
        /// </summary>
        /// <remarks>Setting a value cancels the current and any pending step tasks.</remarks>
        public long Position
        {
            get => step;
            set
            {
                Stop();
                var p = value - step;
                FirstStep(p);
            }
        }

        /// <summary>
        /// Gets or sets the absolute rotational angle in degrees of the motor.
        /// </summary>
        /// <remarks>Setting a value cancels the current and any pending step tasks.</remarks>
        public double Angle
        {
            get => (double)step / SPR * 360d;
            set
            {
                Stop();
                var p = (long)(value / 360d * SPR) - step;
                FirstStep(p);
            }
        }

        /// <summary>
        /// Move the motor by a relative position.
        /// </summary>
        /// <param name="position">The relative position to rotate.</param>
        /// <param name="move">Move to an absolute or relative position.</param>
        /// <param name="rpm">The optional RPM to during the move.</param>
        /// <remarks>Calling this method cancels the current and any pending step tasks.</remarks>
        public Uln2003 MovePosition(long position, StepperMove move = StepperMove.Absolute, 
            double rpm = 0)
        {
            if (rpm > 0)
                RPM = rpm;
            if (move == StepperMove.Absolute)
                Position = position;
            else
                FirstStep(position);
            return this;
        }

        /// <summary>
        /// Move the motor by a relative angle.
        /// </summary>
        /// <param name="angle">The angle to rotate.</param>
        /// <param name="move">Move to an absolute or relative rotational angle.</param>
        /// <param name="rpm">The optional RPM to during the move.</param>
        /// <remarks>Calling this method cancels the current and any pending step tasks.</remarks>
        public Uln2003 MoveAngle(double angle, StepperMove move = StepperMove.Absolute,
            double rpm = 0)
        {
            if (rpm > 0)
                RPM = rpm;
            if (move == StepperMove.Absolute)
                Angle = angle;
            else
            {
                var position = (long)(angle / 360d * SPR);
                FirstStep(position);
            }
            return this;
        }

        /// <summary>
        /// Add a position command to a queue of step tasks.
        /// </summary>
        /// <param name="position">The absolute position to rotate after the current step completes.</param>
        /// <param name="move">Move to an absolute or relative position.</param>
        /// <param name="delay">The optional delay in milliseconds between previous step and the command being issued.</param>
        /// <param name="rpm">The optional RPM to use when the command is processed.</param>
        /// <remarks>If no step task exists then a zero step task will be added first.</remarks>
        public Uln2003 ThenPosition(long position, StepperMove move = StepperMove.Absolute, 
            double delay = 0, double rpm = 0)
        {
            NextStep(position, move, delay, rpm);
            return this;
        }

        /// <summary>
        /// Add an angle command to a queue of step tasks.
        /// </summary>
        /// <param name="angle">The absolute angle to rotate after the current step completes.</param>
        /// <param name="move">Move to an absolute or relative rotational angle.</param>
        /// <param name="delay">The optional delay in milliseconds between previous step and the command being issued.</param>
        /// <param name="rpm">The optional RPM to use when the command is processed.</param>
        /// <remarks>If no step task exists then a zero step task will be added first.</remarks>
        public Uln2003 ThenAngle(double angle, StepperMove move = StepperMove.Absolute, 
            double delay = 0, double rpm = 0)
        {
            var position = (long)(angle / 360d * SPR);
            NextStep(position, move, delay, rpm);
            return this;
        }

        /// <summary>
        /// Stops the motor and resets the step and angle values to zero.
        /// </summary>
        /// <remarks>Calling this method cancels the current and any pending step tasks</remarks>
        public void Zero()
        {
            Stop();
            step = 0;
        }

        /// <summary>
        /// Calculate the closest exact angle supported by the stepping mode, 
        /// </summary>
        /// <param name="angle">The angle to find aligning with the closest step.</param>
        public double Closest(double angle)
        {
            var p = (long)(angle / 360d * SPR);
            return (double)p / SPR * 360d;
        }

        /// <summary>
        /// Occurs when on a step task completes.
        /// </summary>
        public event EventHandler<EventArgs> OnStepTaskComplete;

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