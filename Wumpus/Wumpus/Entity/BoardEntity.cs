using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandEngine;

namespace CSharpPlayground.Wumpus
{
    public abstract class BoardEntity : Component
    {
        public BoardRoom CurrentRoom { get; protected set; }

        public void SetRoom(BoardRoom room)
        {
            var type = this.GetType();
            if (CurrentRoom != null)
                CurrentRoom.RemoveEntity(this, type);
            room.AddEntity(this, type);
            CurrentRoom = room;
        }

        public virtual void Init(BoardRoom room)
        {
            SetRoom(room);
        }

        public override void OnDestroy()
        {
            var type = this.GetType();
            CurrentRoom.RemoveEntity(this, type);
            CurrentRoom = null;
        }
    }
}
