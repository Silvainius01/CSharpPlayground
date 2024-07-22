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
            OnServerStart();

            using (var publisher = new PublisherSocket())
            {
                publisher.Bind($"tcp://*:{port}");

                while (true)
                {
                    foreach (var report in GenerateReports())
                    {
                        if (report.DontPublish)
                            continue;

                        publisher
                            .SendMoreFrame(report.Topic) // Topic
                            .SendFrame(report.Data); // Message

                        if (report.DebugData)
                            Console.WriteLine($"Sent report '{report.Topic}': {report.Data}");
                        else Console.WriteLine($"Sent report '{report.Topic}'");
                    }

                    Thread.Sleep(5000);
                }
            }
        }

        protected abstract bool OnServerStart();
        protected abstract IEnumerable<ServerReport> GenerateReports();

    }
}
