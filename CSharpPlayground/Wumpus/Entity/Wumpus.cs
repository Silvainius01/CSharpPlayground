using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlayground.Wumpus
{
    public enum WUMPUS_DIFFICULTY { EASY, MEDIUM, HARD }
    class Wumpus : BoardEntity
    {
        Timer moveTimer = new Timer(5.0f, true);


        public override void Update()
        {
            if(moveTimer.Update(TimeManager.DeltaTime))
            {
                Console.WriteLine("You hear shuffling in the darkness...");
                moveTimer.Activate();
            }
        }
    }
}
