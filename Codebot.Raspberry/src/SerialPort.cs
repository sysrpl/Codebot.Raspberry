using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
    }

    /// <summary>
    /// The SerialPort class exposes hardware UART control for sending and receiving data
    /// </summary>
    /// <remarks>The Raspberry Pi uses GPIO pins 14 and 15 for TX and RX</remarks>
    public sealed class SerialPort : IDisposable
    {
        #region External library code
        private const int O_RDWR = 0x002;
        private const int O_NOCTTY = 0x100;

        private const uint TCFLSH = 0x540b;

        private const int TCIFLUSH = 0;
        private const int TCIOFLUSH = 2;
        private const int TCOFLUSH = 1;
        private const int TCSANOW = 0;

        private const uint CBAUD = 4111;
        private const uint CBAUDEX = 4096;

        private const uint B300 = 7;
        private const uint B1200 = 9;
        private const uint B2400 = 11;
        private const uint B4800 = 12;
        private const uint B9600 = 13;
        private const uint B19200 = 14;
        private const uint B38400 = 15;
        private const uint B57600 = 4097;
        private const uint B115200 = 4098;
        private const uint B230400 = 4099;

        private const uint CS5 = 0;
        private const uint CS6 = 16;
        private const uint CS7 = 32;
        private const uint CS8 = 48;

        private const uint CSTOPB = 64;

        private const uint PARENB = 256;
        private const uint PARODD = 512;

        private const uint CLOCAL = 2048;
        private const uint CREAD = 128;
        private const uint CSIZE = 48;
        private const uint ECHO = 8;
        private const uint ECHOE = 16;
        private const uint ECHOK = 32;
        private const uint ECHONL = 64;
        private const uint ICANON = 2;
        private const uint ICRNL = 256;
        private const uint IEXTEN = 32768;
        private const uint IGNBRK = 1;
        private const uint IGNCR = 128;
        private const uint INLCR = 64;
        private const uint INPCK = 16;
        private const uint ISIG = 1;
        private const uint ISTRIP = 32;
        private const uint OCRNL = 8;
        private const uint ONLCR = 4;
        private const uint OPOST = 1;

        private const uint VTIME = 6;
        private const uint VMIN = 7;

        [StructLayout(LayoutKind.Explicit)]
        private struct TermiosStruct
        {
            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(0)]
            public uint c_iflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(4)]
            public uint c_oflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(8)]
            public uint c_cflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(12)]
            public uint c_lflag;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            [FieldOffset(16)]
            public byte[] c_cc;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(52)]
            public uint c_ispeed;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(56)]
            public uint c_ospeed;
        }

        [DllImport("libc", EntryPoint = "open")]
        private static extern int ExternalFileOpen(string path, int flags);

        [DllImport("libc", EntryPoint = "close")]
        static private extern int ExternalFileClose(int fd);

        [DllImport("libc", EntryPoint = "write")]
        private static extern int ExternalWrite(int fd, byte[] buffer, int numBytes);

        [DllImport("libc", EntryPoint = "read")]
        private static extern int ExternalRead(int fd, byte[] buffer, int numBytes);

        [DllImport("libc", EntryPoint = "ioctl")]
        private static extern int ExternalIoCtl(int fd, uint request, int value);

        [DllImport("libc", EntryPoint = "tcgetattr")]
        private static extern int ExternalTCGetAttr(int fd, ref TermiosStruct term);

        [DllImport("libc", EntryPoint = "tcsetattr")]
        private static extern int ExternalTCSetAttr(int fd, int actions, ref TermiosStruct term);
        #endregion

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
            port = ExternalFileOpen(device, O_RDWR | O_NOCTTY);
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
            ExternalTCGetAttr(port, ref term);
            term.c_cflag |= CLOCAL | CREAD;
            term.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ISIG | IEXTEN);
            term.c_oflag &= ~(OPOST | ONLCR | OCRNL);
            term.c_iflag &= ~(INLCR | IGNCR | ICRNL | IGNBRK | INPCK | ISTRIP);
            term.c_cflag &= ~(CBAUD | CBAUDEX | CSIZE);
            term.c_cflag |= options.Baud switch
            {
                Baud300 => B300,
                Baud1200 => B1200,
                Baud2400 => B2400,
                Baud4800 => B4800,
                Baud9600 => B9600,
                Baud19200 => B19200,
                Baud38400 => B38400,
                Baud57600 => B57600,
                Baud115200 => B115200,
                Baud230400 => B230400,
                _ => B9600
            };
            term.c_cflag |= options.DataBits switch
            {
                Bits5 => CS5,
                Bits6 => CS6,
                Bits7 => CS7,
                Bits8 => CS8,
                _ => CS8
            };
            if (options.Parity == Parity.Odd)
                term.c_cflag |= PARENB | PARODD;
            else if (options.Parity == Parity.Even)
            {
                term.c_cflag &= ~PARODD;
                term.c_cflag |= PARENB;
            }
            else if (options.Parity == Parity.None)
                term.c_cflag &= ~(PARENB | PARODD);
            if (options.StopBits == StopBits.One)
                term.c_cflag &= ~CSTOPB;
            else
                term.c_cflag |= CSTOPB;
            term.c_cc[VMIN] = options.Min;
            term.c_cc[VTIME] = options.Timeout;
            ExternalTCSetAttr(port, TCSANOW, ref term);
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
            ExternalFileClose(p);
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
                    ExternalIoCtl(port, TCFLSH, TCIFLUSH);
                    break;
                case FlushDirection.Output:
                    ExternalIoCtl(port, TCFLSH, TCOFLUSH);
                    break;
                case FlushDirection.Both:
                    ExternalIoCtl(port, TCFLSH, TCIOFLUSH);
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
                ExternalWrite(port, data, data.Length);
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
            Task.Run(() => ExternalRead(port, buffer, buffer.Length))
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
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
            Task.Run(() => ExternalRead(port, buffer, buffer.Length))
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
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