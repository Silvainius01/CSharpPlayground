using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    class DungeonRoom
    {
        public int Index { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int DistanceToEntrace = -1;

        public bool IsEntrance;
        public bool IsBossRoom;
        public bool HasPathToEntrance;

        public List<DungeonChest> chests;
        public HashSet<int> connections = new HashSet<int>(4);
        public List<Creature> creatures;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
