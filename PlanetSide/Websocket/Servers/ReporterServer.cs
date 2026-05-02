using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Websocket.Client.Logging;
using CommandEngine;
using System.Threading.Tasks;

namespace PlanetSide.Websocket
{
    public enum ServerType { Publisher }
    public abstract class ReportServer
    {
        public bool IsReporting { get; set; }
        public bool DebugEventNames { get; set; }
        public bool DebugEventDetails { get; set; }

        private string port = "56854";
        private ServerType serverType;
        protected static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(ReportServer));

        protected CommandModule serverCommands = new CommandModule("Enter Server Command");
        protected CancellationTokenSource ctServer = new CancellationTokenSource();

        public ReportServer(string port, ServerType type)
        {
            this.port = port;
            serverType = type;

            serverCommands.Add(new ConsoleCommand("stop", CloseServer));
        }

        protected async Task CommandHandlerTask(CancellationToken ct)
        {
            PeriodicTimer t = new PeriodicTimer(TimeSpan.FromMilliseconds(1));

            while (!ct.IsCancellationRequested)
            {
                serverCommands.NextCommand(false);
                await t.WaitForNextTickAsync(ct);
            }
        }

        public void StartServer()
        {
            IsReporting = true;
            OnServerStart();

            using (var publisher = new PublisherSocket())
            {
                publisher.Bind($"tcp://*:{port}");

                while (IsReporting)
                {
                    foreach (var report in GenerateReports())
                    {
                        if (report.DontPublish)
                            continue;

                        publisher
                            .SendMoreFrame(report.Topic) // Topic
                            .SendFrame(report.Data); // Message

                        if (DebugEventDetails)
                            Console.WriteLine($"Sent report '{report.Topic}': {report.Data}");
                        else if (DebugEventNames)
                            Console.WriteLine($"Sent report '{report.Topic}'");
                    }

                    Thread.Sleep(1000);
                }
            }
        }
        public void CloseServer()
        {
            ctServer.Cancel();
        }

        protected abstract bool OnServerStart();
        protected abstract IEnumerable<ServerReport> GenerateReports();

        private void CloseServer(List<string> args)
            => CloseServer();
    }
}
