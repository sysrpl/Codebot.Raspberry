using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Codebot.Raspberry;
using static System.Console;

namespace Tests
{
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

        static void TestSettings(string device)
        {
            WriteLine($"Testing serial port settings on device {device}\n");
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
                        options.FlowControl = FlowControl.XOn | FlowControl.XOff;
                        options.Min = 14;
                        break;
                    case 2:
                        options.Baud = 57600;
                        options.StopBits = StopBits.Two;
                        options.DataBits = 6;
                        options.Parity = Parity.None;
                        options.FlowControl = FlowControl.RequestToSend;
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
                        options.FlowControl = FlowControl.RequestToSend | FlowControl.XOn;
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



        static void TestServer(string device)
        {
            WriteLine($"Testing serial port server on device {device}\n");
            var port = new SerialPort(device);
            var options = DefaultOptions();
            var tick = 0;
            if (port.Open(options))
            {
                WriteLine($"Port was opened with these options\n{options}");
                var running = true;
                var task = Task.Run(() => {
                    while (running)
                    {
                        var text = port.Read();
                        if (text.Length > 0)
                        {
                            Write(text);
                            port.Write($"> received {text.Length} characters / tick {tick++}\n");
                        }
                    }
                });
                WriteLine($"Press enter to stop the server");
                ReadLine();
                running = false;
                task.Wait();
                port.Close();
            }
            else
                WriteLine($"Failed to open device");
        }

        static SerialPortOptions DefaultOptions()
        {
            var options = SerialPortOptions.Default;
            options.Baud = SerialPort.Baud115200;
            options.Parity = Parity.Even;
            options.Min = 0;
            options.Timeout = 1;
            return options;
        }

        static void TestSerialPort(string device)
        {
            WriteLine($"Testing serial port on device {device}\n");
            var port = new SerialPort(device);
            var options = DefaultOptions();
            if (port.Open(options))
            {
                WriteLine($"Port was opened with these options\n{options}");
                var running = true;
                var task = Task.Run(() => {
                    while (running)
                    {
                        var receive = port.Read();
                        if (receive.Length > 0)
                            Write(receive);
                    }
                });
                while (true)
                {
                    WriteLine($"Transmit a text message or type quit to exit");
                    var transmit = ReadLine();
                    if (transmit == "q" || transmit == "quit")
                        break;
                    port.Write(transmit + "\n");
                }
                running = false;
                task.Wait();
                port.Close();
            }
            else
                WriteLine($"Failed to open device");
        }


        static void TestSerialPort()
        {
            string device = File.Exists("/dev/ttyAMA0") ? "/dev/serial0" : "/dev/ttyUSB0";
            WriteLine($"Run test serial (p)ort settings, run as (s)erver, or run as (c)lient?");
            var test = ReadLine().Trim().ToLower();
            if (string.IsNullOrWhiteSpace(test))
                return;
            switch (test[0])
            {
                case 'p':
                    TestSettings(device);
                    break;
                case 's':
                    TestServer(device);
                    break;
                case 'c':
                    TestSerialPort(device);
                    break;
            }
        }

        public static void Run()
        {
            TestSerialPort();
        }
    }
}
