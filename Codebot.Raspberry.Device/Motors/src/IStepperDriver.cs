using System;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// The stepper driver interface provides a generic description of the methods
    /// needed to comminicate with a stepper motor driver.
    /// </summary>
    public interface IStepperDriver : IDisposable
    {
        // Move the motor one step.
        void Step();
        // Set the direction of the movement. Either forward (1) or backwards (-1);
        void SetDirection(int value);
        // Set the stepper mode, defaults to zero.
        void SetMode(int value);
        // Enable or disable the magnetic stepper motor coils.
        void SetEnable(bool value);
        // Retrieve the number of steps per rotation for the current mode.
        int GetSPR();
    }
}