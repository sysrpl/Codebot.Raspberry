#define timer
using System;
#if timer
using System.Timers;
#endif

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// The ServoMotor device class represents a servo motor controlled.
    /// </summary>
    [Device("ServoMotor", "Servo Motor", Category = "Motors",
        Remarks = "Uses hardware PWM chipset and channel")]
    public sealed class ServoMotor : HardwareDevice, IDisposable
    {
        PwmChannel pwm;

        /// <summary>
        /// Create a servo motor using PWM over GPIO pin 18 for communication.
        /// </summary>
        public ServoMotor(double maxAngle = 180d) : this(0, 0, maxAngle)
        {
        }

        /// <summary>
        /// Create a servo motor using PWM with a chipset and channel for communication.
        /// </summary>
        public ServoMotor(int chipset, int channel, double maxAngle = 180d)
        {
#if timer
            enabled = false;
            timerExpired = false;
            timer = new Timer(2000)
            {
                AutoReset = false,
                Enabled = false
            };
            timer.Elapsed += Idle;
#endif
            if (maxAngle < 0d)
                MaxAngle = 0d;
            else if (maxAngle > 360d)
                MaxAngle = 360d;
            else
                MaxAngle = maxAngle;
            pwm = new PwmChannel(chipset, channel)
            {
                Period = 20_000_000,
            };
            PulseWidths(1_000_000, 2_000_000);
        }

        double pulseMin;
        double pulseMax;

        /// <summary>
        /// Redefine the minimum and maxmimum pulse widths.
        /// </summary>
        /// <param name="min">The minumum pulse width in milliseconds.</param>
        /// <param name="max">The maxmimum pulse width in milliseconds.</param>
        /// <remarks>The default min and max are pusle widths are 1ms and 2ms respecfully.</remarks>
        public void PulseWidths(double min, double max)
        {
            pulseMin = min * 1_000_000;
            pulseMax = max * 1_000_000;
            Angle = angle;
        }

#if timer
        void TimerReset()
        {
            lock (this)
            {
                timer.Enabled = false;
                timerExpired = false;
                if (enabled)
                    timer.Enabled = true;
            }
        }

        void Idle(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                timerExpired = true;
                pwm.Disable();
            }
        }

        Timer timer;
        bool timerExpired;
        bool enabled;
#endif

        /// <summary>
        /// Start sending PWM data to the servo motor.
        /// </summary>
        public void Start()
        {
#if timer
            enabled = true;
            TimerReset();
#endif
            pwm.Enable();
        }

        /// <summary>
        /// Stop sending PWM data to the servo motor.
        /// </summary>
        public void Stop()
        {
#if timer
            enabled = false;
            TimerReset();
#endif
            pwm.Disable();
        }

        /// <summary>
        /// The maximum angle range of the servo motor as passed to the constructor.
        /// </summary>
        /// <value>The max angle.</value>
        public double MaxAngle { get; private set; }

        double angle;

        /// <summary>
        /// Position of the servo motor between 0 and MaxAngle.
        /// </summary>
        public double Angle
        {
            get => angle;
            set
            {
                const double EPSILON = 0.0001;
                if (MaxAngle < EPSILON)
                    return;
                var a = value;
                if (a < 0)
                    a = 0;
                if (a > MaxAngle)
                    a = MaxAngle;
                angle = a;
                a = a / MaxAngle;
                a = a * pulseMax + (1 - a) * pulseMin;
                pwm.DutyCycle = a / pwm.Period;
#if timer
                if (enabled)
                {
                    var wasExpired = timerExpired;
                    TimerReset();
                    if (wasExpired)
                        pwm.Enable();
                }
#endif
            }
        }

        bool disposed;

        /// <summary>
        /// Dispose releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;
            Stop();
            disposed = true;
            pwm.Dispose();
        }
    }
}
