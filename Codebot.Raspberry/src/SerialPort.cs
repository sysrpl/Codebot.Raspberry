using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Codebot.Raspberry
{
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

    public enum FlushDirection
    {
        Input,
        Output,
        Both
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

        private const uint TCGETS = 0x5401;
        private const uint TCSETS = 0x5402;
        private const uint TCFLSH = 0x540b;

        private const int TCIFLUSH = 0;
        private const int TCIOFLUSH = 2;
        private const int TCOFLUSH = 1;

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

        [StructLayout(LayoutKind.Explicit)]
        private struct TermiosStruct
        {
            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(0)]
            public UInt32 c_iflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(4)]
            public UInt32 c_oflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(8)]
            public UInt32 c_cflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(12)]
            public UInt32 c_lflag;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(16)]
            public UInt32 c_line;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            [FieldOffset(20)]
            public string c_cc;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(52)]
            public UInt32 c_ispeed;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(56)]
            public UInt32 c_ospeed;
        }

        [DllImport("libc", EntryPoint = "write")]
        private static extern int ExternalWrite(int fd, byte[] buffer, int numBytes);

        [DllImport("libc", EntryPoint = "read")]
        private static extern int ExternalRead(int fd, byte[] buffer, int numBytes);

        [DllImport("libc", EntryPoint = "ioctl")]
        private static extern int ExternalIoCtl(int fd, uint request, int intVal);

        [DllImport("libc", EntryPoint = "ioctl")]
        private static extern int ExternalIoCtl(int fd, uint request, ref TermiosStruct xfer);

        [DllImport("libc", EntryPoint = "open")]
        private static extern int ExternalFileOpen(string path, int flags);

        [DllImport("libc", EntryPoint = "close")]
        static private extern int ExternalFileClose(int fd);
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

        public SerialPort(string device)
        {
            this.device = device;
        }

        /// <summary>
        /// Open the port using a configuration
        /// </summary>
        public bool Open(int baud = Baud9600, int dataBits = Bits8, Parity parity = Parity.None,
            StopBits stopBits = StopBits.One)
        {
            if (IsOpened)
                return false;
            if (!File.Exists(device))
                return false;
            port = ExternalFileOpen(device, O_RDWR | O_NOCTTY);
            if (IsClosed)
                return false;
            UpdatePort(baud, dataBits, parity, stopBits);
            return true;
        }

        /// <summary>
        /// Update the port settings using ioctrl
        /// </summary>
        /// <remarks>See https://github.com/pyserial/pyserial/blob/master/serial/serialposix.py</remarks>
        private void UpdatePort(int baud, int dataBits, Parity parity, StopBits stopBits)
        {
            if (IsClosed)
                return;
            var term = new TermiosStruct();
            ExternalIoCtl(port, TCGETS, ref term);
            term.c_cflag |= CLOCAL | CREAD;
            term.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ISIG | IEXTEN);
            term.c_oflag &= ~(OPOST | ONLCR | OCRNL);
            term.c_iflag &= ~(INLCR | IGNCR | ICRNL | IGNBRK | INPCK | ISTRIP);
            term.c_cflag &= ~(CBAUD | CBAUDEX | CSIZE);
            uint b;
            switch (baud)
            {
                case Baud300:
                    b = B300;
                    break;
                case Baud1200:
                    b = B1200;
                    break;
                case Baud2400:
                    b = B2400;
                    break;
                case Baud4800:
                    b = B4800;
                    break;
                case Baud9600:
                    b = B9600;
                    break;
                case Baud19200:
                    b = B19200;
                    break;
                case Baud38400:
                    b = B38400;
                    break;
                case Baud57600:
                    b = B57600;
                    break;
                case Baud115200:
                    b = B115200;
                    break;
                case Baud230400:
                    b = B230400;
                    break;
                default:
                    b = B9600;
                    break;
            }
            term.c_cflag |= b;
            switch (dataBits)
            {
                case Bits5:
                    term.c_cflag |= CS5;
                    break;
                case Bits6:
                    term.c_cflag |= CS6;
                    break;
                case Bits7:
                    term.c_cflag |= CS7;
                    break;
                default:
                    term.c_cflag |= CS8;
                    break;
            }
            switch (parity)
            {
                case Parity.Odd:
                    term.c_cflag |= PARENB | PARODD;
                    break;
                case Parity.Even:
                    term.c_cflag &= ~PARODD;
                    term.c_cflag |= PARENB;
                    break;
                case Parity.None:
                    term.c_cflag &= ~(PARENB | PARODD);
                    break;
            }
            if (stopBits == StopBits.One)
                term.c_cflag &= ~CSTOPB;
            else
                term.c_cflag |= CSTOPB;
            ExternalIoCtl(port, TCSETS, ref term);
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
            if (IsClosed)
                return;
            var data = Encoding.UTF8.GetBytes(text);
            ExternalWrite(port, data, data.Length);
        }

        /// <summary>
        /// Write binary data
        /// </summary>
        public void WriteBytes(byte[] data)
        {
            if (IsClosed)
                return;
            ExternalWrite(port, data, data.Length);
        }

        /// <summary>
        /// The SerialRead callback returns the text read from the Read method
        /// </summary>
        public delegate void SerialRead(SerialPort port, string text);

        /// <summary>
        /// Read text from the port using a callback when text is ready
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
        /// Read binary data from the port using a callback when data is ready
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