using System;
using System.IO;

namespace Codebot.Raspberry
{
    /// <summary>
    /// PWM channel provides access to hardware PWM commincation using a chip and channel.
    /// </summary>
    /// <remarks>See the files /sys/class/pwm and the url https://jumpnowtek.com/rpi/Using-the-Raspberry-Pi-Hardware-PWM-timers.html</remarks>
    public class PwmChannel : IDisposable
    {
        bool disposed;
        UnixFile enableFile;
        UnixFile periodFile;
        UnixFile dutyCycleFile;

        static void Write(string fileName, string text)
        {
            using (var file = new UnixFile(fileName))
                file.Write(text);
        }

        static void Write(UnixFile file, string text)
        {
            file.Reset();
            file.Write(text);
        }

        /// <summary>
        /// Create a PwmChannel using a chip and channel.
        /// </summary>
        /// <remarks>Gpio pin 18 is on chip 0 and channel 0</remarks>
        public PwmChannel(int chip, int channel)
        {
            disposed = true;
            var chipPath = $"/sys/class/pwm/pwmchip{chip}";
            if (!Directory.Exists(chipPath))
                throw new IOException($"Could not locate directory {chipPath}.");
            var channelPath = $"{chipPath}/pwm{channel}";
            if (!Directory.Exists(channelPath))
                Write($"{chipPath}/export", channel.ToString());
            if (!Directory.Exists(channelPath))
                throw new IOException($"Could not locate directory {channelPath}.");
            var e = $"{channelPath}/enable";
            var p = $"{channelPath}/period";
            var d = $"{channelPath}/duty_cycle";
            if (!File.Exists(e))
                throw new IOException($"File {e} does not exist.");
            if (!File.Exists(p))
                throw new IOException($"File {p} does not exist.");
            if (!File.Exists(d))
                throw new IOException($"File {d} does not exist.");
            enableFile = new UnixFile(e, "w");
            periodFile = new UnixFile(p, "w");
            dutyCycleFile = new UnixFile(d, "w");
            disposed = false;
        }

        ulong period;

        /// <summary>
        /// Gets or sets the period of time for each cycle.
        /// </summary>
        public ulong Period
        {
            get => period;
            set
            {
                period = value;
                Write(periodFile, period.ToString());
                if (dutyCycle > 0)
                    DutyCycle = dutyCycle;
            }
        }

        double dutyCycle;

        /// <summary>
        /// Gets or sets the period of time for each cycle.
        /// </summary>
        public double DutyCycle
        {
            get => dutyCycle;
            set
            {
                dutyCycle = value;
                if (dutyCycle < 0)
                    dutyCycle = 0;
                if (dutyCycle > 1)
                    dutyCycle = 1;
                var d = (ulong)Math.Round(period * DutyCycle);
                Write(dutyCycleFile, d.ToString());
            }
        }

        /// <summary>
        /// Enable sending of PWM data.
        /// </summary>
        /// <remarks>Period must be set prior to enabling the channel.</remarks>
        public void Enable()
        {
            const double EPSILON = 0.0001;
            if (period < 1)
                throw new InvalidOperationException($"Cannot enable PwmChannel when Period is unset.");
            if (DutyCycle < EPSILON)
                Write(dutyCycleFile, "0");
            Write(enableFile, "1");
        }

        /// <summary>
        /// Disable sending of PWM data.
        /// </summary>
        public void Disable()
        {
            Write(enableFile, "0");
        }

        /// <summary>
        /// Dispose releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            Write(enableFile, "0");
            enableFile.Dispose();
            periodFile.Dispose();
            dutyCycleFile.Dispose();
        }
    }
}
