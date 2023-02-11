using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    struct DungeonRoomConnection
    {
        public int index;
        public Direction direction;

        public static implicit operator int(DungeonRoomConnection c) => c.index;

        public override int GetHashCode() => (23 * 31 + index) * 31 + (int)direction;
    }

    /// <summary>
    /// DungeonRooms are expected to only be compared to each other within the same dungeon.
    /// </summary>
    class DungeonRoom : IDungeonObject, IInspectable
    {
        public int ID { get; set; }
        public int Index { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int DistanceToEntrace = -1;
        public string Name { get; set; } = string.Empty;

        public bool IsEntrance;
        public bool IsBossRoom;
        public bool HasPathToEntrance;

        public Dictionary<int, DungeonRoomConnection> connections = new Dictionary<int, DungeonRoomConnection>(4);

        public override int GetHashCode() => ID;

        public bool ConnectedTo(int roomIndex)
            => connections.ContainsKey(roomIndex);
        public bool ConnectedTo(DungeonRoom room)
            => connections.ContainsKey(room.Index);

        public string BriefString()
        {
            throw new NotImplementedException();
        }

        public string InspectString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }

        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
    }
}
