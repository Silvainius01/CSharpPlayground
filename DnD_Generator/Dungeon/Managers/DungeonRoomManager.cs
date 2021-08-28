using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameEngine;

namespace DnD_Generator
{
    class DungeonRoomManager
    {
        public DungeonRoom EntranceRoom;
        public DungeonRoom BossRoom;
        public Vector2Int dimensions;
        public List<DungeonRoom> rooms;
        public Dictionary<int, DungeonRoom> roomLookup;
        public int maxDistance = -1;

        readonly int maxRoomIndex;
        ColorStringBuilder playerMap = new ColorStringBuilder("\nCurrent Map:\n", ConsoleColor.White);
        
        public DungeonRoomManager() { }
        public DungeonRoomManager(int numRooms)
        {
            double numRoomsSqrt = Math.Sqrt(numRooms);
            this.dimensions = new Vector2Int(0, 0);
            this.maxRoomIndex = numRooms - 1;
            this.rooms = new List<DungeonRoom>(numRooms);
            this.roomLookup = new Dictionary<int, DungeonRoom>(numRooms);

            // If num rooms is perfect square, dungeon is square
            if (numRoomsSqrt == Math.Floor(numRoomsSqrt))
                dimensions.X = dimensions.Y = (int)numRoomsSqrt;
            // Otherwise, create a rectangle with just enough slots for the rooms
            else dimensions = new Vector2Int((int)numRoomsSqrt, (int)numRoomsSqrt + 2);
        }

        public DungeonRoom GetRoomById(int roomId)
        {
            if (roomLookup.ContainsKey(roomId))
                return roomLookup[roomId];
            return null;
        }
        public DungeonRoom GetRoomByIndex(int index)
        {
            if (index < 0 || index >= rooms.Count)
                return null;
            return rooms[index];
        }
        public DungeonRoom GetRoom(DungeonRoom room) => GetRoomById(room.ID);

        IEnumerable<DungeonRoom> RoomsFromTopLeft()
        {
            for (int y = dimensions.Y - 1; y >= 0; --y)
            {
                for (int x = 0; x < dimensions.X; ++x)
                {
                    int index = y * dimensions.X + x;
                    if (index > maxRoomIndex)
                        break;
                    yield return (rooms[index]);
                }
            }
        }

        public string DebugString()
        {
            int numDigits = maxDistance >= 10 ? maxDistance.NumDigits() : 2;
            StringBuilder builder = new StringBuilder($"{dimensions.X}x{dimensions.Y} dungeon layout:\n");

            foreach (var room in RoomsFromTopLeft())
            {
                if (room.Index % dimensions.X == 0)
                    builder.Append("\n");

                if (room.DistanceToEntrace < 0)
                    builder.Append($"{room.DistanceToEntrace.ToString($"D{numDigits - 1}")} ");
                else
                    builder.Append($"{room.DistanceToEntrace.ToString($"D{numDigits}")} ");
            }
            return builder.ToString();
        }

        public bool Navigate(DungeonRoom startRoom, DungeonRoom endRoom, out List<DungeonRoom> path)
        {
            bool foundRoom = false;
            Queue<DungeonRoom> roomQueue = new Queue<DungeonRoom>(rooms.Count);
            Dictionary<int, (DungeonRoom prevRoom, int distance)> navData = new Dictionary<int, (DungeonRoom prevRoom, int distance)>();
            path = new List<DungeonRoom>();
            
            if(startRoom.ID == endRoom.ID)
            {
                path.Add(endRoom);
                return false;
            }

            roomQueue.Enqueue(startRoom);
            navData[startRoom.ID] = (null, 0);
            while(roomQueue.Any())
            {
                DungeonRoom currRoom = roomQueue.Dequeue();
                int distance = navData[currRoom.ID].distance+1;

                if (currRoom.ID == endRoom.ID)
                {
                    foundRoom = true;
                    break;
                }

                foreach(var kvp in currRoom.connections)
                {
                    var connection = kvp.Value;
                    DungeonRoom nextRoom = rooms[connection.index];

                    // If navData doesnt exist, or the new distance is better, update entry and queue the room.
                    if (!navData.ContainsKey(nextRoom.ID) || distance < navData[nextRoom.ID].distance)
                    {
                        navData[nextRoom.ID] = (currRoom, distance);
                        roomQueue.Enqueue(nextRoom);
                    }
                }
            }

            if (foundRoom)
            {
                var nav = navData[endRoom.ID];
                path.Capacity = nav.distance;
                path.Add(endRoom);
                while (nav.prevRoom != null)
                {
                    path.Add(nav.prevRoom);
                    nav = navData[nav.prevRoom.ID];
                }
                path.Reverse();
            }
            return foundRoom;
        }

    }
}

