using CommandEngine;
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
        Timer moveTimer = new Timer(1.0f, true);
        Player player;
        int health = WumpusGameSettings.MAX_WUMPUS_HEALTH;

        public void Init(BoardRoom room, Player p)
        {
            base.Init(room);
            player = p;
            WumpusGameManager.WriteLine($"Wumpus Spawned at room {CurrentRoom.index}");
        }

        public override void Update()
        {
            if (moveTimer.Update(TimeManager.DeltaTime))
            {
                moveTimer.Activate();

                // SetRoom(WumpusGameManager.GetRandomConnectedRoom(CurrentRoom));

                if (CurrentRoom.ContainsEntity(player))
                {
                    // WumpusGameManager.WriteLine($"The wumpus killed you!! ({CurrentRoom.index})");
                }
                else
                {
                    // WumpusGameManager.WriteLine($"You hear shuffling in the darkness... ({CurrentRoom.index})");
                }
            }
        }
        public override void OnDestroy()
        {
            CurrentRoom.RemoveEntity(this);
            --WumpusGameManager.wumpusCount;
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
                EngineManager.DestroyEntity(this.entity);
        }
    }
}
