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
        /// <summary> Is set to true after initialization. </summary>
        public bool IsInitialized { get; private set; }
        /// <summary> True while the server has an open and bound socket, false while server is closed. </summary>
        public bool IsActive { get; private set; }
        /// <summary> True while generating and sending reports. </summary>
        public bool IsReporting { get; private set; }

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
            IsActive = false;
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
            IsActive = false;
            Logger.LogInformation("Server initialized.");
        }
        public void StartServer()
        {
            if(!IsInitialized)
            {
                Logger.LogWarning("Server is not initialized.");
                return;
            }
            if(IsActive)
            {
                Logger.LogWarning("Server is already running.");
                return;
            }

            IsActive = true;
            IsReporting = true;
            OnServerStart();
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
            else if (!IsActive)
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
            if (!IsActive)
                return; 

            // Set state flags
            IsActive = false;
            IsReporting = false;
            serverTask.Wait();
            OnServerStop();

            Logger.LogInformation("Server closed.");
        }

        private async Task ServerLoop(CancellationToken ct)
        {
            using PeriodicTimer reportTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            using PeriodicTimer pausedTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

            using (var publisher = new PublisherSocket())
            {
                publisher.Bind($"tcp://*:{port}");

                while (IsActive)
                {
                    // Dont generate reports if we arent sending them.
                    if(!IsReporting)
                    {
                        await pausedTimer.WaitForNextTickAsync(ct);
                        continue;
                    }

                    foreach (var report in GenerateReports())
                    {
                        if (report.DontPublish)
                            continue;

                        publisher
                            .SendMoreFrame(report.Topic) // Topic
                            .SendFrame(report.Data); // Message


                        if (DebugEventDetails)
                            Logger.LogDebug($"Report '{report.Topic}' Data: {report.Data}");
                        else if(DebugEventNames)
                            Logger.LogInformation($"Sent report '{report.Topic}'");
                    }

                    await reportTimer.WaitForNextTickAsync(ct);
                }
            }

            Logger.LogInformation("Server loop resolved");
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
                    case "-d":
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
