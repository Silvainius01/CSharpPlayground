using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace CSharpPlayground.Wumpus
{
    public abstract class BoardEntity : Component
    {
        public BoardRoom CurrentRoom { get; protected set; }

        public void SetRoom(BoardRoom room)
        {
            if (CurrentRoom != null)
                CurrentRoom.RemoveEntity(this);
            room.AddEntity(this);
            CurrentRoom = room;
        }

        public virtual void Init(BoardRoom room)
        {
            SetRoom(room);
        }
    }
}
