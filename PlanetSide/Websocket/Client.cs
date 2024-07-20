using System;
using System.Collections.Generic;
using System.IO;
using EngineIOSharp.Common.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
using SocketIOSharp.Server;

namespace PlanetSide.WebsocketServer
{
    public class Client
    {
        public static void Start()
        {
            SocketIOClient client = new SocketIOClient(new SocketIOClientOption(EngineIOScheme.http, "localhost", 9001));
            InitEventHandlers(client);

            client.Connect();
            Console.WriteLine("Input /exit to close connection.");

            FactionTeam testTeam = new FactionTeam("test", "1", "17", Tracker.Handler);

            string testStats = JsonConvert.SerializeObject(testTeam.TeamStats);


            string line;
            while (!(line = Console.ReadLine()).Equals("/exit"))
            {
                if (line.Equals("testStats"))
                    line = testStats;

                client.Emit("input", line);
                client.Emit("input array", line, line);
            }

            client.Close();

            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }

        static void InitEventHandlers(SocketIOClient client)
        {
            client.On(SocketIOEvent.CONNECTION, () =>
            {
                Console.WriteLine("Connected!");
            });

            client.On(SocketIOEvent.DISCONNECT, () =>
            {
                Console.WriteLine();
                Console.WriteLine("Disconnected!");
            });

            client.On("echo", (Data) =>
            {
                Console.WriteLine("Echo : " + (Data[0].Type == JTokenType.Bytes ? BitConverter.ToString(Data[0].ToObject<byte[]>()) : Data[0]));
            });

            client.On("echo array", (Data) =>
            {
                Console.WriteLine("Echo1 : " + Data[0]);
                Console.WriteLine("Echo2 : " + Data[1]);
            });
        }
    }
}
