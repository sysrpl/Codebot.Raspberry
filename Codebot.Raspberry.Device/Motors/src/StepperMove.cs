namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// The stepper move enumeration is used to set a stepper motor angle or 
    /// position in absolute or relative unts. When a move is made in absolute 
    /// units it's done in relation to the motor's zero angle or position. When
    /// a move is made in relative units it's done in relation to the current
    /// angle or position.
    /// </summary>
    public enum StepperMove
    {
        /// <summary>Use absolute units measured from the zero position.</summary>
        Absolute,
        /// <summary>Use relative units measured from the current position.</summary>
        Relative
    }
}