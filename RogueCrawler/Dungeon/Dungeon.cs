using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;

namespace RogueCrawler
{
    class Dungeon : IInspectable
    {
        public int ChestCount { get; private set; }
        public int CreatureCount { get; private set; }
        public DungeonRoomManager roomManager;
        public DungeonCreatureManager creatureManager;
        public DungeonChestManager chestManager;
        public DungeonGenerationParameters dParams;
        public HashSet<DungeonRoom> pathSet = new HashSet<DungeonRoom>();
        public List<DungeonRoom> navPath = new List<DungeonRoom>();

        bool UpdateMap { get; set; } = true;
        ColorStringBuilder playerMap = new ColorStringBuilder() { TabString = "-- " };

        public Dungeon(DungeonRoomManager rm, DungeonCreatureManager cm, DungeonChestManager chm)
        {
            roomManager = rm;
            creatureManager = cm;
            chestManager = chm;

            CreatureCount = creatureManager.GetObjectCount();
            ChestCount = chestManager.GetObjectCount();
        }

        public string InspectRoomString(DungeonRoom room, string prefix, int tabCount)
        {
            int chestCount = chestManager.GetObjectCount(room);
            int creatureCount = creatureManager.GetObjectCount(room);
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

            if (prefix == string.Empty)
                prefix = $"Room {room.Index} Info:";

            builder.Append(tabCount, prefix);
            
            tabCount++;
            if (creatureCount == 0 && chestCount == 0)
                builder.NewlineAppend(tabCount, $"The room is empty.");
            else
            {
                if (creatureCount > 0)
                {
                    builder.NewlineAppend(tabCount, $"There's {creatureCount} creatures in the room");
                    tabCount++;
                    foreach (var creature in creatureManager.GetObjectsInRoom(room))
                    {
                        builder.NewlineAppend(tabCount, creature.BriefString());
                    }
                    tabCount--;
                }
                if (chestCount > 0)
                {
                    builder.NewlineAppend(tabCount, $"There's {chestCount} chests in the room");
                    tabCount++;
                    foreach (var chest in chestManager.GetObjectsInRoom(room))
                    {
                        builder.NewlineAppend(tabCount, chest.BriefString());
                    }
                    tabCount--;
                }
            }

            builder.NewlineAppend(tabCount, "Connections:");
            tabCount++;
            foreach (var kvp in room.connections)
            {
                var connection = kvp.Value;
                builder.NewlineAppend(tabCount, $"{connection.direction} -> {connection.index}");
            }
            tabCount--;
            tabCount--;

            return builder.ToString();
        }
        public string CheckRoomString(DungeonRoom room, string prefix, int tabCount)
        {
            int chestCount = chestManager.GetObjectCount(room);
            int creatureCount = creatureManager.GetObjectCount(room);
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

            if (prefix == string.Empty)
                prefix = $"Peeking Room {room.Index}:";
            builder.Append(tabCount, prefix);

            tabCount++;
            if (chestCount == 0 && creatureCount == 0)
                builder.NewlineAppend(tabCount, "The room appears empty.");
            else
            {
                if (chestCount > 0)
                    builder.NewlineAppend(tabCount, "Something's shiny over there!");
                if (creatureCount > 0)
                    builder.NewlineAppend(tabCount, "Something is making noise in there...");
            }
            tabCount--;

            return builder.ToString();
        }

        public bool DamageCreature(Creature c, float damage)
        {
            damage = Mathc.Truncate(damage, 1);
            c.Health.AddValue(-damage);
            if (c.Health.Value <= 0)
            {
                c.Inventory.AddItem(c.PrimaryWeapon);
                c.Inventory.ObjectName = $"{c.ObjectName}'s Corpse";
                chestManager.AddObject(c.Inventory, c.CurrentRoom);
                creatureManager.RemoveObject(c, c.CurrentRoom);
                return true;
            }
            return false;
        }
        public void HealAllCreatures(float hitPoints)
        {
            foreach (var creature in creatureManager.GetAllObjects())
            {
                creature.Health.AddValue(hitPoints);
                creature.Mana.AddValue(hitPoints * 2);
                creature.Fatigue.AddValue(hitPoints * 3);
            }
        }

        public bool RoomContainsLoot(DungeonRoom room)
        {
            if (chestManager.GetObjectCount(room) > 0)
                foreach (DungeonChest<IItem> chest in chestManager.GetObjectsInRoom(room))
                    if (chest.ItemCount > 0)
                        return true;
            return false;
        }
        public bool RoomContainsChest(DungeonRoom room)
            => chestManager.GetObjectCount(room) > 0;
        public bool RoomContainsCreature(DungeonRoom room)
            => creatureManager.GetObjectCount(room) > 0;

        public void MovePlayerToRoom(PlayerCharacter player, DungeonRoom room)
        {
            player.CurrentRoom = room;

            UpdateMap |= MarkRoomExplored(player, room);
            UpdateMap |= MarkBorderRooms(player, room);

            if (room.Index > player.FurthestRoomExplored)
                player.FurthestRoomExplored = room.Index;
        }
        public void PlayerCheckRoom(PlayerCharacter player, DungeonRoom room)
        {
            UpdateMap |= MarkRoomChecked(player, room);
        }

        bool MarkRoomExplored(PlayerCharacter player, DungeonRoom room)
        {
            bool retval = false;
            retval |= player.BorderRooms.Remove(room.Index);
            retval |= player.CheckedRooms.Remove(room.Index);
            retval |= player.ExploredRooms.Add(room.Index);
            return retval;
        }
        bool MarkBorderRooms(PlayerCharacter player, DungeonRoom room)
        {
            bool retval = false;
            foreach (var index in room.connections.Keys)
                if (!player.ExploredRooms.Contains(index) && !player.CheckedRooms.Contains(index))
                    retval |= player.BorderRooms.Add(index);
            return retval;
        }
        bool MarkRoomChecked(PlayerCharacter player, DungeonRoom room)
        {
            bool retval = false;
            if (!player.ExploredRooms.Contains(room.Index))
            {
                retval |= player.CheckedRooms.Add(room.Index);
                retval |= player.BorderRooms.Remove(room.Index);
            }
            return retval;
        }

        public ColorStringBuilder BuildPlayerMap(PlayerCharacter player)
        {
            if (!UpdateMap)
            {
                return playerMap;
            }

            int dimensionsX = roomManager.dimensions.X;
            int numDigits = roomManager.maxDistance >= 10 ? roomManager.maxDistance.NumDigits() : 2;
            int furthestRowExplored = (player.FurthestRoomExplored / dimensionsX) + 1;
            //StringBuilder wallBuilder = new StringBuilder(dimensionsX * 2 + dimensionsX - 1);

            bool InUnexploredRow(DungeonRoom room) => room.Index / dimensionsX > furthestRowExplored;
            string RoomString(DungeonRoom room)
            {
                //bool hasEast = false;
                //bool hasSouth = false;
                //foreach (var connection in room.connections)
                //{
                //    hasEast |= connection.Value.direction == Direction.East;
                //    hasSouth |= connection.Value.direction == Direction.South;
                //}

                //if (!hasSouth)
                //    wallBuilder.Append("-- ");
                //else wallBuilder.Append("   ");

                // char trail = hasEast ? '|' : ' ';
                if (room.DistanceToEntrace < 0)
                    return $"{room.Index.ToString($"D{numDigits - 1}")} ";
                return $"{room.Index.ToString($"D{numDigits}")} ";
            }
            ConsoleColor GetCheckedRoomColor(DungeonRoom room)
            {
                if (player.CheckedRooms.Contains(room.Index))
                {
                    if (RoomContainsCreature(room))
                        return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.Red : ConsoleColor.DarkRed;
                    if (RoomContainsChest(room))
                        return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
                }
                return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.White : ConsoleColor.DarkGray;
            }
            ConsoleColor GetExploredRoomColor(DungeonRoom room)
            {
                if (player.CurrentRoom.Index == room.Index)
                    return ConsoleColor.Cyan;

                if (pathSet.Contains(room))
                    return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                if (RoomContainsCreature(room))
                    return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.Red : ConsoleColor.DarkRed;
                if (RoomContainsLoot(room))
                    return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;

                return room.ConnectedTo(player.CurrentRoom) ? ConsoleColor.White : ConsoleColor.DarkGray;
            }

            playerMap.Clear();
            playerMap.Append("\nCurrent Map:\n", ConsoleColor.White);
            //playerMap.Append(dimensionsX, "\n|", ConsoleColor.Gray);
            
            foreach (var room in roomManager.rooms)
            {
                // Dont append anytihng for rows we cannot display
                if (InUnexploredRow(room))
                    continue;

                // Print invisible characters instead of info for unreached rooms
                // NOTE: not a fixed string to accomadate any and all dungeon sizes.
                if (!player.RoomIsExplorable(room))
                    playerMap.Append(RoomString(room), Console.BackgroundColor);
                // if the room is explorable, instead of it's info/color, print ?'s instead
                else if (!player.ExploredRooms.Contains(room.Index))
                {
                    ConsoleColor color = GetCheckedRoomColor(room);
                    playerMap.builder.Capacity += numDigits + 1;
                    for (int i = 0; i < numDigits; ++i)
                        playerMap.Append($"?", color);
                    playerMap.Append($" ", color);
                }
                else
                {
                    playerMap.Append(RoomString(room), GetExploredRoomColor(room));
                }

                // Newline if this is the last room in the row
                if (room.Index % dimensionsX == dimensionsX - 1)
                {
                    playerMap.Append($"\n", ConsoleColor.White);
                    //wallBuilder.Clear();
                }
            }
            return playerMap;
        }

        public bool CanPlayerPathToRoom(PlayerCharacter player, DungeonRoom room)
        {
            if(roomManager.Navigate(player.CurrentRoom, room, out List<DungeonRoom> path))
            {
                navPath = path;
                pathSet = path.ToHashSet();
                return true;
            }
            return false;
        }
        public void ClearPath()
        {
            pathSet.Clear();
            navPath.Clear();
        }

        public string BriefString()
        {
            throw new NotImplementedException();
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

            if (prefix == string.Empty)
                prefix = "Dungeon Stats:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"Dimensions: {roomManager.dimensions}");
            builder.NewlineAppend(tabCount, $"Chest Rate: {dParams.ChestProbability * 100}%");
            builder.NewlineAppend(tabCount, $"Creature Rate: {dParams.CreatureProbability * 100}%");
            tabCount--;
            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
    }
}
