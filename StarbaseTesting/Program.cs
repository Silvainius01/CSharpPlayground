using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using GameEngine;
using System.Net.Http.Headers;
using System.Collections;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Net.Mime;
using StarbaseTesting.AutomaticGuns;

namespace StarbaseTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            TargetingSystem targetingSystem = new TargetingSystem() { bulletSpeed = 300 };

            string result = targetingSystem.ComputeLeadingAngles(new MovingObject()
            {
                pos = new Vector2_64(0, 100),
                dir = new Vector2_64(-1, 0),
                speed = 100,
                name = "Whiteboard"
            });
            Console.WriteLine(result);

            Console.ReadLine();
        }
    }
}

 