using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PlanetSide.WebsocketServer
{
    public class Server : IDisposable
    {
        private Process _consoleProcess;
        private TcpListener _tcpListner;
        private StreamWriter _consoleWriter;
        private StreamReader _consoleReader;

        private TcpClient _localClient;

        public void Start(string ip)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };

            _consoleProcess = Process.Start(psi);
            _consoleWriter = _consoleProcess.StandardInput;
            _consoleReader = _consoleProcess.StandardOutput;

            _tcpListner = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
            _tcpListner.Start();
            _consoleWriter.WriteLine("Started server on 127.0.0.1:80.");

            _localClient = _tcpListner.AcceptTcpClient();

        }

        public void Dispose()
        {
            _consoleProcess.Close();
            _consoleWriter.Close();
            _consoleReader.Close();
            _tcpListner.Stop();
        }

    }
}
