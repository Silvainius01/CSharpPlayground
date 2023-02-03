using System;
using System.Collections.Generic;
using System.Linq;
using CommandEngine;
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
            StarbaseShipDesignHelper ssc = new StarbaseShipDesignHelper();
            CommandModule activeModule = src.srcCommands;

            CommandManager.globalCommands.Add("src", delegate (List<string> arg) { activeModule = src.srcCommands; });
            CommandManager.globalCommands.Add("ssc", delegate (List<string> arg) { activeModule = ssc.sscCommands; });

            while (true)
            {
                activeModule.NextCommand(true);
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

/*
updateRecipe "Turret Cradle Advanced" r 19 y 3 p 17 b 0
addRecipe "Autocannon Fixed Mount" Bas 7582.14 Cha 5784.92 Vok 5643.13 Nhu 1836 Aeg 1111.6 Exo 300 Ice 204 r 571 b 1177 p 439
addRecipe "Autocannon Barrel" Cha 1987.95 Vok 1902.22 Bas 1426.65 r 108 b 438
addRecipe "Autocannon Magazine" Bas 1627.93 Vok 542.64 r 216
addRecipe "Autocannon Magazine (Refill)" Nhu 1836 Vok 575 Exo 300 Ice 204 r 145 b 29
addRecipe "Autocannon Magazine (Full)" Bas 1627.93 Vok 1117.64 Nhu 1836 Exo 300 Ice 204 r 361 b 29
 */

//thrustCalc 7550825.5 -c 426 -nt 60