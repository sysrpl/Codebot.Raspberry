using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using ColorServer.Network;

#pragma warning disable RECS0063

namespace ColorServer
{
    public static class Program
    {
        const int ServerPort = 7050;    

        static void Run()
        {
            using (var server = new ListenerSocket(ServerPort))
            {
                server.Accept += ServerAccept;
                server.Active = true;
                Console.WriteLine("Color server is running. Press enter to quit.");
                Console.ReadLine();
            }
        }

        static void ServerAccept(object sender, SocketAcceptEventArgs e)
        {
            var s = e.Socket;
            Console.WriteLine("Accepted connection from " +
                IPAddress.Parse(((IPEndPoint)s.RemoteEndPoint).Address.ToString()) +
                " on port number " + ((IPEndPoint)s.RemoteEndPoint).Port.ToString());
            var client = new ClientSocket(1000);
            client.Receive += ClientReceive;
            client.Connect(s);
        }

        static string RunExternal(string fileName, string args)
        {
            using (var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                p.Start();
                StringBuilder s = new StringBuilder();
                while (!p.StandardOutput.EndOfStream)
                    s.AppendLine(p.StandardOutput.ReadLine());
                return s.ToString().Trim();
            }
        }

        static void ClientReceive(object sender, SocketReceiveEventArgs e)
        {
            Console.WriteLine("Received request");
            const string request = "[color] ";
            var client = sender as ClientSocket;
            var s = e.Text;
            if (s.StartsWith(request))
            {
                s = s.Replace(request, "").Trim();
                s = RunExternal("color.py", s);
                Console.WriteLine(s);
                client.Send(s);
            }
            else
            {
                client.Send("[error] invalid request");
            }
        }

        public static void Main(string[] args)
        {
            Run();
        }
    }
}
