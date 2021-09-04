using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine;
using StarbaseTesting.AutomaticGuns;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Data.SqlTypes;

namespace StarbaseTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            OzzySrc src = new OzzySrc();

            while(true)
            {
                src.srcCommands.NextCommand(true);
            }
        }

        static void TargetSystemTest()
        {
            TargetingSystem targetingSystem = new TargetingSystem() { bulletSpeed = 300 };

            string result = targetingSystem.ComputeLeadingAngles(new MovingObject()
            {
                pos = new Vector2_64(0, 100),
                dir = new Vector2_64(0.0f, 0),
                speed = 100,
                name = "Whiteboard"
            });
            Console.WriteLine(result);
        }
    }
}

 