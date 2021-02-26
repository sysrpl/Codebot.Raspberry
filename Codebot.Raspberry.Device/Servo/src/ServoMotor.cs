using System;

namespace Codebot.Raspberry.Device
{
    /// <summary>
    /// The ServoMotor device class represents a servo motor controlled.
    /// </summary>
    [Device("ServoMotor", "Servo Motor", Category = "Motors",
        Remarks = "Uses hardware PWM chipset and channel")]
    public sealed class ServoMotor : HardwareDevice, IDisposable
    {

        const double step = 1d / 20d;

        PwmChannel pwm;

        /// <summary>
        /// Create a servo motor using PWM over GPIO pin 18 for communication.
        /// </summary>
        public ServoMotor(double maxAngle = 180d, ulong period = 20000) : this(0, 0, maxAngle, period)
        {
        }

        /// <summary>
        /// Create a servo motor using PWM with a chipset and channel for communication.
        /// </summary>
        public ServoMotor(int chipset, int channel, double maxAngle = 180d, ulong period = 20000)
        {
            if (maxAngle < 0d)
                MaxAngle = 0d;
            else if (maxAngle > 360d)
                MaxAngle = 360d;
            else
                MaxAngle = maxAngle;
            pwm = new PwmChannel(chipset, channel)
            {
                Period = period,
                DutyCycle = step
            };
        }

        /// <summary>
        /// Start sending PWM data to the servo motor.
        /// </summary>
        public void Start()
        {
            pwm.Enable();
        }

        /// <summary>
        /// Stop sending PWM data to the servo motor.
        /// </summary>
        public void Stop()
        {
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
                var a = value;
                if (a < 0)
                    a = 0;
                if (a > MaxAngle)
                    a = MaxAngle;
                angle = a;
                pwm.DutyCycle = a / MaxAngle * step + step;
            }
        }

        bool disposed;

        /// <summary>
        /// Releases all resource used by this object.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            pwm.Dispose();
        }
    }
}
