namespace Codebot.Raspberry
{
    /// <summary>
    /// Pin event handler arguments are given to consumers of pin voltage 
    /// risings and falling events.
    /// </summary>
    public class PinEventHandlerArgs
    {
        public PinEventHandlerArgs(GpioPin pin, PinEdge edge, bool bounced)
        {
            Pin = pin;
            Edge = edge;
            Bounced = bounced;
        }

        /// <summary>
        /// The pin where the rising or falling voltage was triggered.
        /// </summary>
        public GpioPin Pin { get; private set; }
        /// <summary>
        /// The kind of event, either a rising or falling edge.
        /// </summary>
        public PinEdge Edge { get; private set; }
        /// <summary>
        /// Bounced is true when a rising or falling voltage event is triggered 
        /// due to multiple mechanical contacts of a switch as it settles on an 
        /// opened or closed state.
        /// </summary>
        public bool Bounced { get; private set; }
    }
}
