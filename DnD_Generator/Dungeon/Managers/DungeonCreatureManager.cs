using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    class DungeonCreatureManager : DungeonObjectManager<Creature>
    {
        public DungeonCreatureManager(DungeonRoomManager rm) : base(rm) { }
    }
}
