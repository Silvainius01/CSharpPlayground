using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace PlanetSide.Websocket
{
    public enum ServerType { Publisher }
    public abstract class ReportServer
    {
        private string port = "56854";
        private ServerType serverType;

        public ReportServer(string port, ServerType type)
        {
            this.port = port;
            serverType = type;
        }

        public void StartServer()
        {
            using (var publisher = new PublisherSocket())
            {
                publisher.Bind($"tcp://*:{port}");

                while (true)
                {
                    ServerReport[] reports = GenerateReports();

                    foreach (var report in reports)
                    {
                        publisher
                            .SendMoreFrame(report.Topic) // Topic
                            .SendFrame(report.Data); // Message
                        Console.WriteLine($"Sent report '{report.Topic}'");
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        protected abstract bool OnServerStart();
        protected abstract ServerReport[] GenerateReports();

    }
}
