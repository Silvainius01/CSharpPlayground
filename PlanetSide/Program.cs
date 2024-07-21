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
            module.Add(new ConsoleCommand("client", StartSocketClient));
            module.Add(new ConsoleCommand("cam", WarpgateCam));

            while (true)
            {
                module.NextCommand(false);
            }
        }

        private static void StartTracker(List<string> args)
        {
            Tracker.StartTrackerDebug();
        }

        private static void StartSocketServer(List<string> args)
        {
            Server.StartPublishingServer();
        }
        private static void StartSocketClient(List<string> args)
        {
            Client.Start();
        }

        private static void WarpgateCam(List<string> args)
        {
            SmartStringBuilder builder = new SmartStringBuilder();

            Vector3 FromHeading(double heading)
                => new Vector3(Math.Cos(heading), 0, Math.Sin(heading));

            double westWarpHeight = 24.250;
            double southWarpHeight = 27.260;
            double camWestHeight = 374.544;
            double camHeightRelative = camWestHeight - westWarpHeight;

            Vector3 westWarpCenter = new Vector3(2046.600, 0, -3122.980);
            Vector3 westWarpSpawn = new Vector3(1926.690, 0, -3004.080);
            Vector3 westWarpDirection = Vector3.Direction(westWarpSpawn, westWarpCenter);

            Vector3 camWest = new Vector3(1716.047, 0, -2795.857);
            Vector3 camWestRelative = camWest - westWarpCenter;
            Vector3 camWestDirection = camWestRelative.Normal();
            double camWestMagnitude = camWestRelative.Magnitude;

            Vector3 camWestNew = westWarpDirection * camWestMagnitude;
            camWestNew.Y = westWarpHeight + camHeightRelative;
            camWestNew += westWarpCenter;

            Vector3 southWarpCenter = new Vector3(-3302.800, 0, -86.440);
            Vector3 southWarpSpawn = new Vector3(-3120.670, 0, -45.070);
            Vector3 southWarpDirection = Vector3.Direction(southWarpSpawn, southWarpCenter);

            Vector3 camSouth = southWarpDirection * camWestMagnitude;
            camSouth.Y = camHeightRelative + southWarpHeight;
            camSouth += southWarpCenter;

            builder.NewlineAppend("");
            builder.NewlineAppend($"camWestOld: {camWest}");
            builder.NewlineAppend(1, $"P: {camWest}");
            builder.NewlineAppend(1, $"M: {camWestMagnitude}");
            builder.NewlineAppend($"camWestNew: {camWestNew}");
            builder.NewlineAppend(1, $"P: {camWestNew}");
            builder.NewlineAppend(1, $"M: {(camWestNew - westWarpCenter).Magnitude}");
            builder.NewlineAppend($"camSouthNew: ");
            builder.NewlineAppend(1, $"P: {camSouth}");
            builder.NewlineAppend(1, $"M: {(camSouth - southWarpCenter).Magnitude}");

            Console.WriteLine(builder.ToString());
        }
    }
}
