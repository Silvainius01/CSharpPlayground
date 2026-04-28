using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommandEngine;
using System.Linq;

namespace RogueCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            DungeonCrawlerManager crawlerGame = new DungeonCrawlerManager();
            TestSuite testSuite = new TestSuite(crawlerGame);

            while (true)
            {
                //crawlerGame.UpdateLoop();
                testSuite.NextTestCommand();
            }
        }
    }
}
