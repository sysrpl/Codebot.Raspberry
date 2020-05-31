using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Codebot.Raspberry
{
    /// <summary>
    /// The SimpleSerialPort class exposes hardware UART control for
    /// sending and receiving data
    /// </summary>
    /// <remarks>The Raspberry Pi uses GPIO pins 14 and 15 for TX and RX</remarks>
    public sealed class SimpleSerialPort : IDisposable
    {
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

        public const int Bits7 = 7;
        public const int Bits8 = 8;

        private readonly string device;
        private readonly byte[] buffer = new byte[1024];
        private FileStream stream;

        /// <summary>
        /// Create a SerialPort using the default device /dev/serial0
        /// </summary>
        public SimpleSerialPort() : this("/dev/serial0")
        {
        }

        /// <summary>
        /// Create a SerialPort by specifying the device
        /// </summary>
        public SimpleSerialPort(string device)
        {
            this.device = device;
        }

        /// <summary>
        /// Open the port using a configuration
        /// </summary>
        public bool Open(int baud = Baud9600, int dataBits = Bits8, Parity parity = Parity.None,
            StopBits stopBits = StopBits.One)
        {
            const string flags = "-brkint -icrnl -imaxbel -opost -onlcr -isig -icanon " +
                "-iexten -echo -echoe -echok -echoctl -echoke";
            if (stream is null)
            {
                if (!File.Exists(device))
                    return false;
                stream = File.Open(device, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                string p;
                if (parity == Parity.Even)
                    p = "parenb -parodd";
                else if (parity == Parity.Odd)
                    p = "parenb parodd";
                else
                    p = "-parenb";
                string s;
                if (stopBits == StopBits.One)
                    s = "-cstopb";
                else
                    s = "cstopb";
                Process
                    .Start($"/bin/stty", $"-F {device} {baud} cs{dataBits} {p} {s} {flags}")
                    .WaitForExit();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Close the port cancelling any pending reads or writes
        /// </summary>
        public bool Close()
        {
            if (stream is null)
                return false;
            else
            {
                var s = stream;
                stream = null;
                s.Close();
                return true;
            }
        }

        /// <summary>
        /// Write text
        /// </summary>
        public void Write(string text)
        {
            if (IsClosed)
                return;
            var b = Encoding.UTF8.GetBytes(text);
            stream.Write(b, 0, b.Length);
            stream.Flush();
        }

        /// <summary>
        /// Write binary data
        /// </summary>
        public void WriteBytes(byte[] data)
        {
            if (IsClosed)
                return;
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        /// <summary>
        /// The SerialRead callback returns the text read from the Read method
        /// </summary>
        public delegate void SerialRead(SimpleSerialPort port, string text);

        /// <summary>
        /// Read text from the port using a callback when text is ready
        /// </summary>
        public void Read(SerialRead readComplete)
        {
            stream.ReadAsync(buffer, 0, buffer.Length).ContinueWith(task =>
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
        public delegate void SerialReadBytes(SimpleSerialPort port, byte[] data, int length);

        /// <summary>
        /// Read binary data from the port using a callback when data is ready
        /// </summary>
        public void ReadBytes(SerialReadBytes readComplete)
        {
            if (IsOpened)
                stream.ReadAsync(buffer, 0, buffer.Length).ContinueWith(task =>
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
        public bool IsOpened { get => !(stream is null); }

        /// <summary>
        /// True when the port is closed
        /// </summary>
        public bool IsClosed { get => stream is null; }

        /// <summary>
        /// You can dispose of the port by closing it
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}