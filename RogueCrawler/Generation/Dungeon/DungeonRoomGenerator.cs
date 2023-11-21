using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    class DungeonRoomGenerator : BaseDungeonObjectGenerator<DungeonRoom, DungeonGenerationParameters>
    {
        /// <summary>
        /// Assumes that both rooms are in the same dungeon
        /// </summary>
        public static IEqualityComparer<DungeonRoom> DungeonRoomEquality =
            CommandEngine.EqualityComparerFactory.Create<DungeonRoom>((Func<DungeonRoom, int>)(room => (int)room.Index), (Func<DungeonRoom, DungeonRoom, bool>)((room1, room2) => room1.Index == room2.Index));

        public DungeonRoomManager GenerateDungeonRooms(DungeonGenerationParameters dParams)
        {
            dParams.Validate();

            int numRooms = CommandEngine.Random.NextInt(dParams.RoomRange, true);
            DungeonRoomManager roomManager = new DungeonRoomManager(numRooms);

            // Create and add the rooms
            for (int i = 0; i < numRooms; ++i)
            {
                DungeonRoom room = Generate(dParams);
                room.Index = i;
                roomManager.rooms.Add(room);
                roomManager.roomLookup.Add(room.Index, room);
            }



            // Generate Connections 
            GenerateDungeonConnections(roomManager, dParams);

            // Pick a random "front" room to be the entrance
            var entranceRoom = roomManager.rooms[CommandEngine.Random.NextInt(roomManager.dimensions.X)];
            entranceRoom.IsEntrance = entranceRoom.HasPathToEntrance = true;
            entranceRoom.DistanceToEntrace = 0;
            roomManager.EntranceRoom = entranceRoom;

            GenerateRoomDistances(roomManager);
            RemoveIsolatedRooms(roomManager);

            return roomManager;
        }

        public override DungeonRoom Generate(DungeonGenerationParameters dParams)
        {
            DungeonRoom room = new DungeonRoom
            {
                ID = NextId,
                Width = CommandEngine.Random.NextInt(dParams.RoomWidthRange, true),
                Height = CommandEngine.Random.NextInt(dParams.RoomHeightRange, true),
            };
            return room;
        }

        void GenerateDungeonConnections(DungeonRoomManager dungeon, DungeonGenerationParameters dParams)
        {
            List<DungeonRoomConnection> sideRoomConnections = new List<DungeonRoomConnection>(4);
            for (int roomIndex = 0; roomIndex < dungeon.rooms.Count; roomIndex++)
            {
                DungeonRoom room = dungeon.rooms[roomIndex];
                int numConnections = CommandEngine.Random.NextInt(dParams.ConnectionRange);

                // Add connections to meet the desired amount
                if (room.connections.Count < numConnections)
                {
                    // Add valid sides
                    sideRoomConnections.Clear();
                    for (int j = 0; j < 4; ++j)
                    {
                        var sideInfo = GetSideInfo(dungeon, roomIndex, j);
                        if (sideInfo.valid)
                            sideRoomConnections.Add(sideInfo.connection);
                    }

                    // Eliminate sides until we hit the determined connections
                    while (sideRoomConnections.Count > numConnections)
                        sideRoomConnections.RemoveAt(CommandEngine.Random.NextInt(sideRoomConnections.Count));

                    // Generate connections
                    foreach (var connection in sideRoomConnections)
                    {
                        GenerateRoomConnection(dungeon, roomIndex, connection);
                    }
                }
            }
        }

        void GenerateRoomConnection(DungeonRoomManager dungeon, int index1, DungeonRoomConnection connection)
        {
            Direction oppDirection = (Direction)(((int)connection.direction + 2) % 4);
            dungeon.rooms[index1].connections.Add(connection.index, connection);
            dungeon.rooms[connection].connections.Add(index1, new DungeonRoomConnection() { index = index1, direction = oppDirection});
        }

        void GenerateRoomDistances(DungeonRoomManager dungeon, int startIndex = -1)
        {
            Queue<int> roomIndexQueue = new Queue<int>(dungeon.rooms.Count);
            roomIndexQueue.Enqueue(startIndex < 0 ? dungeon.EntranceRoom.Index : startIndex);

            while (roomIndexQueue.Count > 0)
            {
                // Pull current room from q
                int index = roomIndexQueue.Dequeue();
                DungeonRoom currRoom = dungeon.rooms[index];
                int currDistance = currRoom.DistanceToEntrace + 1;

                if (dungeon.maxDistance < currDistance)
                    dungeon.maxDistance = currDistance;

                foreach (var kvp in currRoom.connections)
                {
                    var connection = kvp.Value;
                    DungeonRoom connectedRoom = dungeon.rooms[connection];

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
        (List<DungeonRoom> BorderRooms, List<DungeonRoom> IsolatedRooms) GetIsolatedRooms(DungeonRoomManager dungeon, IEnumerable<DungeonRoom> roomSubset = null)
        {
            List<DungeonRoom> borderRooms = new List<DungeonRoom>();
            List<DungeonRoom> isolatedRooms = new List<DungeonRoom>();
            roomSubset = roomSubset ?? dungeon.rooms;

            foreach (var room in roomSubset)
                if (!room.HasPathToEntrance)
                {
                    bool isIsolated = true;
                    for (int side = 0; isIsolated && side < 4; ++side)
                    {
                        var sideInfo = GetSideInfo(dungeon, room.Index, side);
                        if (sideInfo.valid && dungeon.rooms[sideInfo.connection].HasPathToEntrance)
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

        public void RemoveIsolatedRooms(DungeonRoomManager dungeon)
        {
            (IEnumerable<DungeonRoom> BorderRooms, IEnumerable<DungeonRoom> IsolatedRooms) = GetIsolatedRooms(dungeon);

            while (BorderRooms.Any())
            {
                var randomRoom = BorderRooms.RandomItem();
                List<DungeonRoomConnection> borderIndexList = new List<DungeonRoomConnection>(4);

                for (int side = 0; side < 4; ++side)
                {
                    var sideInfo = GetSideInfo(dungeon, randomRoom.Index, side);
                    if (sideInfo.valid && dungeon.rooms[sideInfo.connection].HasPathToEntrance)
                        borderIndexList.Add(sideInfo.connection);
                }

                var rBorderConnection = borderIndexList.RandomItem();
                GenerateRoomConnection(dungeon, randomRoom.Index, rBorderConnection);
                GenerateRoomDistances(dungeon, rBorderConnection);
                // Recombine border and isolate room sets, then rerun GetIsolatedRooms()
                (BorderRooms, IsolatedRooms) = GetIsolatedRooms(dungeon, BorderRooms.Union(IsolatedRooms, DungeonRoomEquality));
            }
        }

        (DungeonRoomConnection connection, bool valid) GetSideInfo(DungeonRoomManager dungeon, int index, int side)
        {
            DungeonRoomConnection connection = new DungeonRoomConnection();
            bool valid = false;
            switch (side)
            {
                // Left
                case 0:
                    connection.index = index - 1;
                    connection.direction = Direction.West;
                    valid = index - 1 > 0 && index % dungeon.dimensions.X > 0; 
                    break;
                // Right
                case 1:
                    connection.index = index + 1;
                    connection.direction = Direction.East;
                    valid = index + 1 < dungeon.rooms.Count && index % dungeon.dimensions.X < dungeon.dimensions.X - 1;
                    break;
                // Up (map for the player is upside down)
                case 2:
                    connection.index = index + dungeon.dimensions.X;
                    connection.direction = Direction.South;
                    valid = index + dungeon.dimensions.X < dungeon.rooms.Count;
                    break;
                // Down (map for the player is upside down)
                case 3:
                    connection.index = index - dungeon.dimensions.X;
                    connection.direction = Direction.North;
                    valid = index - dungeon.dimensions.X > 0; 
                    break;
            } 

            // Considered invalid if already connected
            if (dungeon.rooms[index].connections.ContainsKey(connection))
                valid = false;

            return (connection, valid);
        }
    }
}
