using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    class Dungeon
    {
        public List<DungeonRoom> rooms;
        public DungeonRoom EntranceRoom;
        public DungeonRoom BossRoom;
        public Vector2Int dimensions;

        int maxRoomIndex;
        public int maxDistance = -1;

        public Dungeon() { }
        public Dungeon(int numRooms)
        {
            double numRoomsSqrt = Math.Sqrt(numRooms);
            this.dimensions = new Vector2Int(0, 0);
            this.maxRoomIndex = numRooms-1;
            this.rooms = new List<DungeonRoom>(numRooms);

            // If num rooms is perfect square, dungeon is square
            if (numRoomsSqrt == Math.Floor(numRoomsSqrt)) 
                dimensions.X = dimensions.Y = (int)numRoomsSqrt;
            // Otherwise, create a rectangle with just enough slots for the rooms
            else dimensions = new Vector2Int((int)numRoomsSqrt, (int)numRoomsSqrt + 2);
        }

        public string DebugString()
        {
            StringBuilder builder = new StringBuilder($"{dimensions.X}x{dimensions.Y} dungeon layout:\n");

            // i = Y*Dx + X

            int numDigits = maxDistance >= 10 ? maxDistance.NumDigits() : 2;

            if (rooms != null)
                maxRoomIndex = rooms.Count-1;

            for (int y = dimensions.Y - 1; y >= 0; --y)
            {
                for (int x = 0; x < dimensions.X; ++x)
                {
                    int index = y * dimensions.X + x;

                    if (index > maxRoomIndex)
                        break;

                    var room = rooms[index];

                    if (room.DistanceToEntrace < 0)
                        builder.Append($"{room.DistanceToEntrace.ToString($"D{numDigits - 1}")} ");
                    else
                        builder.Append($"{room.DistanceToEntrace.ToString($"D{numDigits}")} ");
                    //   Console.WriteLine($"{index} : ({x}, {y})");
                }
                builder.Append("\n");
            }
            return builder.ToString();
        }
    }
}
