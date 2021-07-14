using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Codebot.Raspberry.Libc;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The read or write buffer to flush
    /// </summary>
    public enum FlushDirection
    {
        Input,
        Output,
        Both
    }

    /// <summary>
    /// The parity to use when opening a serial port
    /// </summary>
    public enum Parity
    {
        Even,
        Odd,
        None
    }

    /// <summary>
    /// The number of stop bits to use when opening a serial port
    /// </summary>
    public enum StopBits
    {
        One,
        Two
    }

    /// <summary>
    /// Options to be used when opening a serial port
    /// </summary>
    public sealed class SerialPortOptions
    {
        /// <summary>
        /// The default options to be used to open a port if no options are given
        /// </summary>
        /// <remarks>See http://www.delorie.com/djgpp/doc/libc/libc_816.html</remarks>
        public static readonly SerialPortOptions Default = new SerialPortOptions
        {
            Baud = SerialPort.Baud9600,
            DataBits = SerialPort.Bits8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Min = 1,
            Timeout = 0
        };

        /// <summary>
        /// The transfer speed in bits per second
        /// </summary>
        public int Baud { get; set; }

        /// <summary>
        /// The character size in bits
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// The type of parity check to append to each character
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// The number of stop bits to apply to each character
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// The minimum number of characters for a completed read
        /// </summary>
        public byte Min { get; set; }

        /// <summary>
        /// The read timeout of in tenths of a second
        /// </summary>
        public byte Timeout { get; set; }

        public override string ToString()
        {
            var properties = GetType().GetProperties();
            var builder = new StringBuilder();
            foreach (var info in properties)
            {
                var value = info.GetValue(this, null) ?? "(null)";
                builder.AppendLine(info.Name + ": " + value);
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// The SerialPort class exposes hardware UART control for sending and receiving data
    /// </summary>
    /// <remarks>The Raspberry Pi uses GPIO pins 14 and 15 for TX and RX</remarks>
    public sealed class SerialPort : IDisposable
    {
        const uint CBAUD = 4111;
        const uint CBAUDEX = 4096;

        const uint B300 = 7;
        const uint B1200 = 9;
        const uint B2400 = 11;
        const uint B4800 = 12;
        const uint B9600 = 13;
        const uint B19200 = 14;
        const uint B38400 = 15;
        const uint B57600 = 4097;
        const uint B115200 = 4098;
        const uint B230400 = 4099;

        const uint CS5 = 0;
        const uint CS6 = 16;
        const uint CS7 = 32;
        const uint CS8 = 48;

        const uint CSTOPB = 64;

        const uint PARENB = 256;
        const uint PARODD = 512;

        const uint CLOCAL = 2048;
        const uint CREAD = 128;
        const uint CSIZE = 48;
        const uint ECHO = 8;
        const uint ECHOE = 16;
        const uint ECHOK = 32;
        const uint ECHONL = 64;
        const uint ICANON = 2;
        const uint ICRNL = 256;
        const uint IEXTEN = 32768;
        const uint IGNBRK = 1;
        const uint IGNCR = 128;
        const uint INLCR = 64;
        const uint INPCK = 16;
        const uint ISIG = 1;
        const uint ISTRIP = 32;
        const uint OCRNL = 8;
        const uint ONLCR = 4;
        const uint OPOST = 1;

        const uint VTIME = 5;
        const uint VMIN = 6;

        public const int Baud300 = 300;
        public const int Baud1200 = 1200;
        public const int Baud2400 = 2400;
        public const int Baud4800 = 4800;
        public const int Baud9600 = 9600;
        public const int Baud19200 = 19200;
        public const int Baud38400 = 38400;
        public const int Baud57600 = 57600;
        public const int Baud115200 = 115200;
        public const int Baud230400 = 230400;

        public const int Bits5 = 5;
        public const int Bits6 = 6;
        public const int Bits7 = 7;
        public const int Bits8 = 8;

        private readonly string device;
        private readonly byte[] buffer = new byte[1024];
        private int port;

        /// <summary>
        /// Create a SerialPort using the default device /dev/serial0
        /// </summary>
        public SerialPort() : this("/dev/serial0")
        {
        }

        /// <summary>
        /// Create a SerialPort using the specified device
        /// </summary>
        public SerialPort(string device)
        {
            this.device = device;
        }

        /// <summary>
        /// Open the port using the default options
        /// </summary>
        public bool Open()
        {
            return Open(SerialPortOptions.Default);
        }

        /// <summary>
        /// Open the port using a specified set of options
        /// </summary>
        public bool Open(SerialPortOptions options)
        {
            if (IsOpened)
                return false;
            if (!File.Exists(device))
                return false;
            port = open(device, O_RDWR | O_NOCTTY);
            if (IsClosed)
                return false;
            UpdatePort(options);
            return true;
        }

        /// <summary>
        /// Update the port settings using a set of options
        /// </summary>
        /// <remarks>See https://github.com/pyserial/pyserial/blob/master/serial/serialposix.py</remarks>
        private void UpdatePort(SerialPortOptions options)
        {
            var term = new TermiosStruct();
            tcgetattr(port, ref term);
            term.c_cflag |= CLOCAL | CREAD;
            term.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ISIG | IEXTEN);
            term.c_oflag &= ~(OPOST | ONLCR | OCRNL);
            term.c_iflag &= ~(INLCR | IGNCR | ICRNL | IGNBRK | INPCK | ISTRIP);
            term.c_cflag &= ~(CBAUD | CBAUDEX | CSIZE);
            switch (options.Baud)
            {
                case (Baud300):
                    term.c_cflag |= B300;
                    break;
                case (Baud1200):
                    term.c_cflag |= B1200;
                    break;
                case (Baud2400):
                    term.c_cflag |= B2400;
                    break;
                case (Baud4800):
                    term.c_cflag |= B4800;
                    break;
                case (Baud9600):
                    term.c_cflag |= B9600;
                    break;
                case (Baud19200):
                    term.c_cflag |= B19200;
                    break;
                case (Baud38400):
                    term.c_cflag |= B38400;
                    break;
                case (Baud57600):
                    term.c_cflag |= B57600;
                    break;
                case (Baud115200):
                    term.c_cflag |= B115200;
                    break;
                case (Baud230400):
                    term.c_cflag |= B230400;
                    break;
                default:
                    term.c_cflag |= B9600;
                    break;
            }
            switch (options.DataBits)
            {
                case (Bits5):
                    term.c_cflag |= CS5;
                    break;
                case (Bits6):
                    term.c_cflag |= CS6;
                    break;
                case (Bits7):
                    term.c_cflag |= CS7;
                    break;
                case (Bits8):
                    term.c_cflag |= CS8;
                    break;
                default:
                    term.c_cflag |= CS8;
                    break;
            }
            if (options.Parity == Parity.Odd)
                term.c_cflag |= PARENB | PARODD;
            else if (options.Parity == Parity.Even)
            {
                term.c_cflag &= ~PARODD;
                term.c_cflag |= PARENB;
            }
            else
                term.c_cflag &= ~(PARENB | PARODD);
            if (options.StopBits == StopBits.One)
                term.c_cflag &= ~CSTOPB;
            else
                term.c_cflag |= CSTOPB;
            term.c_cc5 = options.Timeout;
            term.c_cc6 = options.Min; 
            tcsetattr(port, TCSANOW, ref term);
        }

        /// <summary>
        /// Close the port cancelling any pending reads or writes
        /// </summary>
        public bool Close()
        {
            if (IsClosed)
                return false;
            var p = port;
            port = 0;
            close(p);
            return true;
        }

        /// <summary>
        /// Flush reads or writes or both
        /// </summary>
        public void Flush(FlushDirection direction)
        {
            if (IsClosed)
                return;
            switch (direction)
            {
                case FlushDirection.Input:
                    ioctl(port, TCFLSH, TCIFLUSH);
                    break;
                case FlushDirection.Output:
                    ioctl(port, TCFLSH, TCOFLUSH);
                    break;
                case FlushDirection.Both:
                    ioctl(port, TCFLSH, TCIOFLUSH);
                    break;
            }
        }

        /// <summary>
        /// Write text
        /// </summary>
        public void Write(string text)
        {
            if (IsOpened)
                WriteBytes(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Write binary data
        /// </summary>
        public void WriteBytes(byte[] data)
        {
            if (IsOpened)
                write(port, data, data.Length);
        }

        /// <summary>
        /// The SerialRead callback returns the text read from the Read method
        /// </summary>
        public delegate void SerialRead(SerialPort port, string text);

        /// <summary>
        /// Read text from the port using a callback when text is available
        /// </summary>
        public void Read(SerialRead readComplete)
        {
            if (IsClosed)
                return;
            Task.Run(() => read(port, buffer, buffer.Length))
                .ContinueWith(task =>
                {
                    if (task.IsCompleted && !(task.IsCanceled || task.IsFaulted))
                    {
                        var c = task.Result;
                        if (c < 0)
                            c = 0;
                        string s = string.Empty;
                        if (c > 0)
                            s = Encoding.UTF8.GetString(buffer, 0, c);
                        if (IsOpened)
                            readComplete(this, s);
                    }
                });
        }

        /// <summary>
        /// The SerialReadBytes callback returns the bytes read from the ReadBytes method
        /// </summary>
        public delegate void SerialReadBytes(SerialPort port, byte[] data, int length);

        /// <summary>
        /// Read binary data from the port using a callback when data is available
        /// </summary>
        public void ReadBytes(SerialReadBytes readComplete)
        {
            if (IsClosed)
                return;
            Task.Run(() => read(port, buffer, buffer.Length))
                .ContinueWith(task =>
                {
                    if (task.IsCompleted && !(task.IsCanceled || task.IsFaulted))
                    {
                        var c = task.Result;
                        if (c < 0)
                            c = 0;
                        if (IsOpened)
                            readComplete(this, buffer, c);
                    }
                });
        }

        /// <summary>
        /// True when the port is opened
        /// </summary>
        public bool IsOpened { get => port > 0; }

        /// <summary>
        /// True when the port is closed
        /// </summary>
        public bool IsClosed { get => port < 1; }

        /// <summary>
        /// You can dispose of the port by closing it
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}