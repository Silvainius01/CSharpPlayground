using System;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace PlanetSide.WebsocketServer
{
    public class SubscriptionClient
    {
        public static void Start()
        {
            using (var subscriber = new SubscriberSocket())
            {
                subscriber.Connect("tcp://localhost:56854");
                subscriber.Subscribe("");

                while (true)
                {
                    var topic = subscriber.ReceiveFrameString();
                    var msg = subscriber.ReceiveFrameString();
                    Console.WriteLine("From Publisher: {0} {1}", topic, msg);
                }
            }
        }
    }
}
