using System;
using GameEngine;
using GameEngine.AI;
using CSharpPlayground.Wumpus;
using System.Collections;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Text;
using System.Security.Principal;

using System.Windows;

namespace CSharpPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            EngineManager engineManager = new EngineManager();
            WumpusGameManager gameManager = WumpusGameManager.CreateInstance(new DefaultBoardGenerator(5, 5, false));

            while (WumpusGameManager.WindowOpen)
            {
                EngineManager.FrameUpdate();
            }
        }
    }
}

 