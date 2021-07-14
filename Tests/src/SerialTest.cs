using System.Diagnostics;
using System.Text;
using Codebot.Raspberry;
using static System.Console;

namespace Tests
{
    // Note:
    // To make /dev/serial0 map to /dev/ttyAMA0 
    // add dtoverlay=pi3-miniuart-bt to /boot/config.txt
    public static class SerialTest
    {
        static string RunExternal(string program, string arguments)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            StringBuilder s = new StringBuilder();
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
                s.AppendLine(proc.StandardOutput.ReadLine());
            return s.ToString();
        }

        static void ReadData(SerialPort port, string text)
        {
            if (text.Length > 0)
            {
                WriteLine($"Received: {text}");
                port.Write($"I recevied {text.Length} characters");
            }
            port.Read(ReadData);
        }

        static void TestSerialListen()
        {
            WriteLine($"Enter the serial device name (e.g /dev/serial0)");
            var device = ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(device))
                return;
            WriteLine($"Listening serial port on device {device}\n");

            var port = new SerialPort(device);
            var options = SerialPortOptions.Default;
            options.Baud = 9600;
            options.Parity = Parity.Even;
            options.Min = 255;
            options.Timeout = 5;
            if (port.Open(options))
            {
                WriteLine($"Port was opened and listening with options:\n{options}");
                port.Read(ReadData);
                WriteLine("Press return to stop listening");
                ReadLine();
                port.Close();
            }
            else
            {
                WriteLine($"Failed to open device");
            }
        }

        static void TestSerialConfig()
        {
            WriteLine($"Enter the serial device name (e.g /dev/ttyAMA0)");
            var device = ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(device))
                return;
            WriteLine($"Testing serial port on device {device}\n");
            var port = new SerialPort(device);
            for (var i = 0; i < 5; i++)
            {
                var options = SerialPortOptions.Default;
                switch (i)
                {
                    case 0:
                        options.Baud = 9600;
                        options.StopBits = StopBits.One;
                        options.DataBits = 8;
                        options.Parity = Parity.Odd;
                        break;
                    case 1:
                        options.Baud = 1200;
                        options.StopBits = StopBits.One;
                        options.DataBits = 7;
                        options.Parity = Parity.Even;
                        options.Min = 14;
                        break;
                    case 2:
                        options.Baud = 57600;
                        options.StopBits = StopBits.Two;
                        options.DataBits = 6;
                        options.Parity = Parity.None;
                        options.Min = 2;
                        options.Timeout = 2;
                        break;
                    case 3:
                        options.Baud = 115200;
                        options.StopBits = StopBits.One;
                        options.DataBits = 8;
                        options.Parity = Parity.Even;
                        options.Min = 1;
                        options.Timeout = 0;
                        break;
                    case 4:
                        options.Baud = 19200;
                        options.StopBits = StopBits.Two;
                        options.DataBits = 6;
                        options.Parity = Parity.Odd;
                        options.Min = 10;
                        options.Timeout = 20;
                        break;
                }
                if (port.Open(options))
                {
                    port.Close();
                    WriteLine($"Port was opened with options:\n{options}");
                    WriteLine($"Verify options:");
                    WriteLine(RunExternal("/bin/stty", $"-F {device} -a"));
                    ReadLine();
                }
                else
                {
                    WriteLine($"Failed to open device");
                    break;
                }
            }
        }

        public static void Run()
        {
            TestSerialListen();
        }
    }
}
