using System;
using System.Collections.Generic;
using System.ComponentModel;
using DieRoller;
using GameEngine;
using System.Linq;

namespace DnD_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            DungeonCrawlerManager crawlerGame = new DungeonCrawlerManager();
            TestSuite testSuite = new TestSuite(crawlerGame);

            while (true)
            {
                crawlerGame.UpdateLoop();
                //testSuite.NextTestCommand();
            }
        }
    }
}
