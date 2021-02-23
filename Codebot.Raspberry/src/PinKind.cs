namespace Codebot.Raspberry
{
    public enum PinKind
    {
        /// <summary>
        /// A pin is set to input making only reading available on the pin .
        /// </summary>
        Input,
        /// <summary>
        /// The same as input, but the pin state is low unless input voltage is more than ground.
        /// </summary>
        InputPullDown,
        /// <summary>
        /// The same as output, but the pin state is high unless input voltage is ground.
        /// </summary>
        InputPullUp,
        /// <summary>
        /// A pin is set to output making only writing available on the pin.
        /// </summary>
        Output,
        /// <summary>
        /// Pin mode is none if the pin is not a valid logical pin.
        /// </summary>
        None
    }
}