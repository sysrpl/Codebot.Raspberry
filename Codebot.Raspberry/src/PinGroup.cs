using System.Collections.Generic;
using PinHandler = System.EventHandler<Codebot.Raspberry.PinEventHandlerArgs>;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The pin group class allows multiple pins to share rising or falling events.
    /// </summary>
    public class PinGroup
    {

        readonly List<GpioPin> pins;
        public double now;

        public PinGroup(params GpioPin[] pins)
        {
            this.pins = new List<GpioPin>();
            this.pins.AddRange(pins);
            now = 0;
        }

        /// <summary>
        /// The time in millisecond to assume under which limit an input event 
        /// was a bounce rather than a legitimate voltage rise or fall.
        /// </summary>
        public double BounceDelay { get; set; } = 30;

        List<PinHandler> rising;

        void Rising(object sender, PinEventHandlerArgs args)
        {
            if (!args.Bounced)
            {
                var n = Pi.Now;
                var b = true;
                if (n - now > BounceDelay)
                {
                    now = n;
                    b = false;
                }
                args.Bounced = b;
            }
            foreach (var handler in rising)
                handler(this, args);
        }

        /// <summary>
        /// The OnRisingEdge can be used to trigger an event handler when a pin 
        /// is an input kind and the voltage rises above ground.
        /// </summary>
        public event PinHandler OnRisingEdge
        {
            add
            {
                if (rising is null)
                {
                    rising = new List<PinHandler>();
                    foreach (var p in pins)
                        p.OnRisingEdge += Rising;
                }
                rising.Add(value);
            }
            remove
            {
                rising.Remove(value);
                if (rising.Count < 1)
                {
                    foreach (var p in pins)
                        p.OnRisingEdge -= Rising;
                    rising = null;
                }
            }
        }

        List<PinHandler> falling;

        void Falling(object sender, PinEventHandlerArgs args)
        {
            if (!args.Bounced)
            {
                var n = Pi.Now;
                var b = true;
                if (n - now > BounceDelay)
                {
                    now = n;
                    b = false;
                }
                args.Bounced = b;
            }
            foreach (var handler in falling)
                handler(this, args);
        }

        /// <summary>
        /// The OnFallingEdge can be used to trigger an event handler when a pin 
        /// is an input kind and the voltage drops to ground.
        /// </summary>
        public event PinHandler OnFallingEdge
        {
            add
            {
                if (falling is null)
                {
                    falling = new List<PinHandler>();
                    foreach (var p in pins)
                        p.OnFallingEdge += Falling;
                }
                falling.Add(value);
            }
            remove
            {
                falling.Remove(value);
                if (falling.Count < 1)
                {
                    foreach (var p in pins)
                        p.OnFallingEdge -= Falling;
                    falling = null;
                }
            }
        }

        /// <summary>
        /// Remove any connected rising or falling event handlers.
        /// </summary>
        public void RemoveEvents()
        {
            if (!(rising is null))
            {
                foreach (var p in pins)
                    p.OnRisingEdge -= Rising;
                rising = null;
            }
            if (!(falling is null))
            {
                foreach (var p in pins)
                    p.OnFallingEdge -= Falling;
                falling = null;
            }
        }
    }
}
