using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CommandEngine;
using Microsoft.Extensions.Logging;
using PlanetSide.WebsocketServer;

namespace PlanetSide
{
    public class Program
    {
        public static readonly ILoggerFactory LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());

        private static readonly ILogger<Program> Logger = LoggerFactory.CreateLogger<Program>();

        static CommandModule module = new CommandModule("Enter Start Up Command");

        private static void Main(string[] args)
        {
            module.Add(new ConsoleCommand("tracker", StartTracker));
            module.Add(new ConsoleCommand("server", StartSocketServer));
            module.NextCommand(false);
        }

        private static void StartTracker(List<string> args)
        {
            Tracker.StartTracker();
        }

        private static void StartSocketServer(List<string> args)
        {
            Server.Start();
        }
    }
}
