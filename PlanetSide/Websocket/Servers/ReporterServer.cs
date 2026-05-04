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
using System.ComponentModel;

namespace PlanetSide.Websocket
{
    public enum ServerType { Publisher }
    public abstract class ReportServer
    {
        public bool IsInitialized { get; private set; }
        public bool IsReporting { get; private set; }
        public bool IsClosed { get; private set; }

        public bool DebugEventNames { get; set; }
        public bool DebugEventDetails { get; set; }

        public CommandModule serverCommands { get; protected set; }

        protected static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(ReportServer));
        
        private string port = "56854";
        private ServerType serverType;
        private Task serverTask;
        private CancellationTokenSource ctServer = new CancellationTokenSource();

        public ReportServer(string port, ServerType type)
        {
            this.port = port;
            serverType = type;
            IsClosed = true;
            IsReporting = false;
            IsInitialized = false;

            serverCommands = new CommandModule("Enter Server Command");
            serverCommands.Add(new ConsoleCommand("start", StartServerCommand));
            serverCommands.Add(new ConsoleCommand("stop", StopServerCommand));
            serverCommands.Add(new ConsoleCommand("pause", PauseServerCommand));
            serverCommands.Add(new ConsoleCommand("debug", DebugServerCommand));
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            OnInitialize();
            IsInitialized = true;
            Logger.LogInformation("Server initialized.");
        }
        public void StartServer()
        {
            if(!IsInitialized)
            {
                Logger.LogWarning("Server is not initialized.");
                return;
            }
            if(!IsClosed)
            {
                Logger.LogWarning("Server is already running.");
                return;
            }

            OnServerStart();
            IsClosed = false;
            IsReporting = true;
            serverTask = Task.Run(() => ServerLoop(ctServer.Token), ctServer.Token);
            Logger.LogInformation("Server started on port {0}.", port);
        }
        public void PauseServer()
        {
            if(!IsInitialized)
            {
                Logger.LogWarning("Server is not initialized.");
                return;
            }
            else if (IsClosed)
            {
                Logger.LogWarning("Cannot pause a closed server.");
                return;
            }

            OnServerPause();
            IsReporting = false;
        }
        public void CloseServer()
        {
            if(!IsInitialized)
            {
                Logger.LogWarning("Server is not initialized.");
                return;
            }
            if (IsClosed)
                return; 

            ctServer.Cancel();
            serverTask.Wait();
            IsReporting = false;
            OnServerStop();
            IsClosed = true;
            Logger.LogInformation("Server closed.");
        }

        private async Task ServerLoop(CancellationToken ct)
        {
            PeriodicTimer reportTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));

            using (var publisher = new PublisherSocket())
            {
                publisher.Bind($"tcp://*:{port}");

                while (!ct.IsCancellationRequested)
                {
                    foreach (var report in GenerateReports())
                    {
                        if (!IsReporting || report.DontPublish)
                            continue;

                        publisher
                            .SendMoreFrame(report.Topic) // Topic
                            .SendFrame(report.Data); // Message

                        if (DebugEventDetails)
                            Logger.LogDebug($"Sent report '{report.Topic}': {report.Data}");
                        else if (DebugEventNames)
                            Logger.LogDebug($"Sent report '{report.Topic}'");
                    }

                    await reportTimer.WaitForNextTickAsync(ct);
                }
            }
        }

        protected abstract void OnInitialize();
        protected abstract void OnServerStop();
        protected abstract void OnServerStart();
        protected abstract void OnServerPause();
        protected abstract IEnumerable<ServerReport> GenerateReports();

        private void StartServerCommand(List<string> args)
        {
            bool pauseOnStart = false;

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-p":
                    case "-pause":
                        pauseOnStart = true;
                        break;
                }
            }

            StartServer();
            if (pauseOnStart)
                PauseServer();
        }
        private void StopServerCommand(List<string> args)
        {
            CloseServer();
        }
        private void PauseServerCommand(List<string> args)
        {
            PauseServer();
        }
        private void DebugServerCommand(List<string> args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                string? arg = args[i];

                switch (arg)
                {
                    case "-n":
                    case "-en":
                    case "-eventNames":
                        if (i < args.Count - 1 && CommandManager.TryParseBoolean(args[i + 1], out bool bv1))
                        {
                            ++i; // Skip the next argument since it's the value for this flag
                            DebugEventNames = bv1;
                        }
                        Logger.LogInformation($"DebugEventNames: {DebugEventNames}");
                        break;
                    case "-d":
                    case "-ed":
                    case "-eventDetails":
                        if (i < args.Count - 1 && CommandManager.TryParseBoolean(args[i + 1], out bool bv2))
                        {
                            ++i; // Skip the next argument since it's the value for this flag
                            DebugEventDetails = bv2;
                        }
                        Logger.LogInformation($"Set DebugEventDetails: {DebugEventDetails}");
                        break;
                }
            }
        }
    }
}
