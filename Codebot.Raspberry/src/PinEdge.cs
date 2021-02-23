namespace Codebot.Raspberry
{
    /// <summary>
    /// Pin edge is used to register and respond to rising and falling voltage
    /// events on a Gpio pin.
    /// </summary>
    public enum PinEdge
    {
        /// <summary>
        /// Voltage of the pin had risen to a high state.
        /// </summary>
        Rising,
        /// <summary>
        /// Voltage of the pin has fallen to a low state.
        /// </summary>
        Falling
    }
}
