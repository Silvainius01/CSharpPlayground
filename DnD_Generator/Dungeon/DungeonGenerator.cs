using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameEngine;

namespace DnD_Generator
{
    class DungeonGenerationParameters
    {
        public Vector2Int ConnectionRange { get; set; }
        public Vector2Int RoomRange { get; set; }
        public Vector2Int DungeonCreatureRange { get; set; }
        public Vector2Int RoomCreatureRange { get; set; }
        public Vector2Int RoomHeightRange { get; set; }
        public Vector2Int RoomWidthRange { get; set; }
        public float ChestProbability { get; set; } = 0.1f;

        public void Validate()
        {
            int temp = Mathc.Max(2, RoomRange.X);
            RoomRange = new Vector2Int(temp, Mathc.Max(temp, RoomRange.Y));
        }
    }

    class DungeonGenerator
    {
        static Random rng = Mathc.Random.RNG;
        /// <summary>
        /// Assumes that both rooms are in the same dungeon
        /// </summary>
        public static IEqualityComparer<DungeonRoom> DungeonRoomEquality =
            GameEngine.EqualityComparerFactory.Create<DungeonRoom>(room => room.Index, (room1, room2) => room1.Index == room2.Index);

        public static Dungeon GenerateDungeon(DungeonGenerationParameters dParams)
        {
            int numRooms = rng.Next(dParams.RoomRange.X, dParams.RoomRange.Y);
            Dungeon dungeon = new Dungeon(numRooms);

            // Create and add the rooms
            for (int i = 0; i < numRooms; ++i)
            {
                dungeon.rooms.Add(GenerateRoom(dParams, i));
            }

            // Generate Connections 
            GenerateDungeonConnections(dungeon, dParams);

            // Pick a random "front" room to be the entrance
            var entranceRoom = dungeon.rooms[rng.Next(dungeon.dimensions.X)];
            entranceRoom.IsEntrance = entranceRoom.HasPathToEntrance = true;
            entranceRoom.DistanceToEntrace = 0;
            dungeon.EntranceRoom = entranceRoom;
            
            GenerateRoomDistances(dungeon);
            RemoveIsolatedRooms(dungeon);

            return dungeon;
        }

        static void GenerateDungeonConnections(Dungeon dungeon, DungeonGenerationParameters dParams)
        {
            List<int> sideRoomIndexs = new List<int>(4);
            for (int roomIndex = 0; roomIndex < dungeon.rooms.Count; roomIndex++)
            {
                DungeonRoom room = dungeon.rooms[roomIndex];
                int numConnections = Mathc.Random.NextInt(dParams.ConnectionRange);

                // Add connections to meet the desired amount
                if (room.connections.Count < numConnections)
                {
                    // Add valid sides
                    sideRoomIndexs.Clear();
                    for (int j = 0; j < 4; ++j)
                    {
                        var sideInfo = GetSideInfo(dungeon, roomIndex, j);
                        if (sideInfo.valid)
                            sideRoomIndexs.Add(sideInfo.sideIndex);
                    }

                    // Eliminate sides until we hit the determined connections
                    while (sideRoomIndexs.Count > numConnections)
                        sideRoomIndexs.RemoveAt(rng.Next(sideRoomIndexs.Count));

                    // Generate connections
                    foreach (int sideIndex in sideRoomIndexs)
                    {
                        GenerateRoomConnection(dungeon, roomIndex, sideIndex);
                    }
                }
            }
        }

        static void GenerateRoomConnection(Dungeon dungeon, int index1, int index2)
        {
            dungeon.rooms[index1].connections.Add(index2);
            dungeon.rooms[index2].connections.Add(index1);
        }

        static void GenerateRoomDistances(Dungeon dungeon, int startIndex = -1)
        {
            Queue<int> roomIndexQueue = new Queue<int>(dungeon.rooms.Count);
            roomIndexQueue.Enqueue(startIndex < 0 ? dungeon.EntranceRoom.Index : startIndex);

            while(roomIndexQueue.Count > 0)
            {
                // Pull current room from q
                DungeonRoom currRoom = dungeon.rooms[roomIndexQueue.Dequeue()];
                int currDistance = currRoom.DistanceToEntrace + 1;

                if (dungeon.maxDistance < currDistance)
                    dungeon.maxDistance = currDistance;

                foreach(int index in currRoom.connections)
                {
                    DungeonRoom connectedRoom = dungeon.rooms[index];
                    
                    // Add room to q, mark as accessible, and record distance if:
                    //   1) Room has not been reached yet OR
                    //   2) Current distance marker is smaller than the recorded one
                    if (!connectedRoom.HasPathToEntrance || connectedRoom.DistanceToEntrace > currDistance)
                    {
                        connectedRoom.HasPathToEntrance = true;
                        connectedRoom.DistanceToEntrace = currDistance;
                        roomIndexQueue.Enqueue(connectedRoom.Index);
                    }
                }    
            }
        }

        /// <summary>
        /// Returns two lists of insaccessable rooms:
        ///     <para>BorderRooms is the set of rooms that border an accessable room</para>
        ///     <para>IsolatedRooms is the set of rooms that DO NOT border an accesable room</para>
        /// </summary>
        /// <param name="dungeon">The dungeon that roomSubset exists in.</param>
        /// <param name="roomSubset">The set of rooms you'd like to check, defaults to all rooms within the dungeon.</param>
        static (List<DungeonRoom> BorderRooms, List<DungeonRoom> IsolatedRooms) GetIsolatedRooms(Dungeon dungeon, IEnumerable<DungeonRoom> roomSubset = null)
        {
            List<DungeonRoom> borderRooms = new List<DungeonRoom>();
            List<DungeonRoom> isolatedRooms = new List<DungeonRoom>();
            roomSubset = roomSubset ?? dungeon.rooms;

            foreach(var room in roomSubset)
                if(!room.HasPathToEntrance)
                {
                    bool isIsolated = true;
                    for (int side = 0; isIsolated && side < 4; ++side)
                    {
                        var sideInfo = GetSideInfo(dungeon, room.Index, side);
                        if (sideInfo.valid && dungeon.rooms[sideInfo.sideIndex].HasPathToEntrance)
                        {
                            borderRooms.Add(room);
                            isIsolated = false;
                        }
                    }

                    if (isIsolated)
                        isolatedRooms.Add(room);
                }
            return (borderRooms, isolatedRooms);
        }

        public static void RemoveIsolatedRooms(Dungeon dungeon)
        {
            (IEnumerable<DungeonRoom> BorderRooms, IEnumerable<DungeonRoom> IsolatedRooms) = GetIsolatedRooms(dungeon);

            while(BorderRooms.Any())
            {
                var randomRoom = Mathc.GetRandomItemFromEnumerable(BorderRooms);
                List<int> borderIndexList = new List<int>(4);

                for (int side = 0; side < 4; ++side)
                {
                    var sideInfo = GetSideInfo(dungeon, randomRoom.Index, side);
                    if (sideInfo.valid && dungeon.rooms[sideInfo.sideIndex].HasPathToEntrance)
                        borderIndexList.Add(sideInfo.sideIndex);
                }

                int rBorderIndex = Mathc.GetRandomItemFromList(borderIndexList);
                GenerateRoomConnection(dungeon, randomRoom.Index, rBorderIndex);
                GenerateRoomDistances(dungeon, rBorderIndex);
                // Recombine border and isolate room sets, then rerun GetIsolatedRooms()
                (BorderRooms, IsolatedRooms) = GetIsolatedRooms(dungeon, BorderRooms.Union(IsolatedRooms, DungeonGenerator.DungeonRoomEquality));
            }
        }

        static (int sideIndex, bool valid) GetSideInfo(Dungeon dungeon, int index, int side)
        {
            (int sideIndex, bool valid) retval = (index, false);
            switch (side)
            {
                // Left
                case 0: retval = (index - 1, index - 1 > 0 && index % dungeon.dimensions.X > 0); break;
                // Right
                case 1: retval = (index + 1, index + 1 < dungeon.rooms.Count && index % dungeon.dimensions.X < dungeon.dimensions.X - 1); break;
                // Up
                case 2: retval = (index + dungeon.dimensions.X, index + dungeon.dimensions.X < dungeon.rooms.Count); break;
                // Down
                case 3: retval = (index - dungeon.dimensions.X, index - dungeon.dimensions.X > 0); break;
            }

            // Considered invalid if already connected
            if (dungeon.rooms[index].connections.Contains(retval.sideIndex))
                retval.valid = false;

            return retval;
        }

        static DungeonRoom GenerateRoom(DungeonGenerationParameters dParams, int index)
        {
            DungeonRoom room = new DungeonRoom
            {
                Index = index,
                Width = rng.Next(dParams.RoomWidthRange.X, dParams.RoomWidthRange.Y),
                Height = rng.Next(dParams.RoomHeightRange.X, dParams.RoomHeightRange.Y)
            };

            if(Mathc.Random.NextFloat() < dParams.ChestProbability)
            {
                DungeonChestGenerationProperties chestProperties = new DungeonChestGenerationProperties()
                {
                    ChestQuality = DungeonChestQuality.Any,
                    ChestType = DungeonChestType.Weapon,
                    ChestLootGrade = DungeonChestLootGrade.Any
                };
                room.chests.Add(DungeonChestGenerator.GenerateChest(chestProperties));
            }

            return room;
        }
    }
}
