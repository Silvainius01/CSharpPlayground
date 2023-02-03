using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;

namespace DnD_Generator
{
    class TestSuite
    {
        int total = 0;
        bool isRepeating = false;
        Dictionary<int, int> rCount = new Dictionary<int, int>(10)
        {
            [0] = 0,
            [1] = 0,
            [2] = 0,
            [3] = 0,
            [4] = 0,
            [5] = 0,
            [6] = 0,
            [7] = 0,
            [8] = 0,
            [9] = 0
        };
        Dictionary<int, int> rCount2 = new Dictionary<int, int>(10)
        {
            [0] = 0,
            [1] = 0,
            [2] = 0,
            [3] = 0,
            [4] = 0,
            [5] = 0,
            [6] = 0,
            [7] = 0,
            [8] = 0,
            [9] = 0
        };
        Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();
        DungeonCrawlerManager crawlerGame;

        public TestSuite(DungeonCrawlerManager crawler)
        {
            crawlerGame = crawler;
            commands.Add(ConsoleCommand.Create("weapon", WeaponTest));
            commands.Add(ConsoleCommand.Create("dungeon", DungeonTest));
            commands.Add(ConsoleCommand.Create("rand", RandomTest));
            commands.Add(ConsoleCommand.Create("mod", ModTest));
            commands.Add(ConsoleCommand.Create("creature", CreatureTest));
            commands.Add(ConsoleCommand.Create("chest", ChestTest));
            commands.Add(ConsoleCommand.Create("map", MapTest));
            commands.Add(ConsoleCommand.Create("xp", ExpTest));
            commands.Add(ConsoleCommand.Create("nav", NavTest));
        }
        public void NextTestCommand()
        {
            CommandManager.GetNextCommand("\nEnter Next Test Command", true, commands);
        }

        bool BaseRepeatableCommand(int repeatArgIndex, List<string> args)
        {
            if (isRepeating)
                return Console.ReadLine().Length == 0;

            if (args.Count > 0 && args[repeatArgIndex][0] == 'r')
            {
                args.RemoveAt(repeatArgIndex);
                isRepeating = true;
                return true;
            }
            return false;
        }

        public void RandomTest(List<string> args)
        {
            int numRolls = 100000;
            float rFloat = 0.0f;
            for (int i = 0; i < numRolls; ++i)
            {
                rFloat = (float)Mathc.Random.NextFloat();
                if (rFloat < 0.2f)
                {
                    total += 1;
                    ++rCount[(int)(rFloat * 100) % 2];
                }
                //++rCount2[(int)rFloat % 2];
                //Console.WriteLine(rFloat);
            }
            for (int i = 0; i < 10; ++i)
            {
                Console.WriteLine($"{i}: {(rCount[i] / (float)total) * 100} vs {(rCount2[i] / (float)total) * 100}");
                //Console.WriteLine($"{i}: {(rCount[i] / (float)total) * 100}");
            }

            if (args.Count > 0 && args[0][0] == 'r')
            {
                args.RemoveAt(0);
                while (Console.ReadLine().Length == 0)
                    RandomTest(args);
            }
        }

        public void ModTest(List<string> args)
        {
            string[] modOptionsRaw = args.ToArray();
            float toMod, modBase;

            if (modOptionsRaw.Length > 1 && float.TryParse(modOptionsRaw[1], out modBase))
            {
                while (modOptionsRaw[0] != "e")
                {
                    if (float.TryParse(modOptionsRaw[0], out toMod))
                        Console.WriteLine($"{toMod} % {modBase} = {Mathc.Mod(toMod, modBase)}");
                    else Console.WriteLine($"{modOptionsRaw[0]} is a valid float.");

                    modOptionsRaw = Console.ReadLine().Split(' ');
                }
            }
        }

        public void DungeonTest(List<string> args)
        {
            //Dungeon d = new Dungeon(26);

            DungeonGenerationParameters dParams = new DungeonGenerationParameters()
            {
                PlayerLevel = 6,
                RoomRange = new Vector2Int(10 * 9, 10 * 10),
                ConnectionRange = new Vector2Int(1, 3),
                RoomHeightRange = new Vector2Int(1, 1),
                RoomWidthRange = new Vector2Int(1, 1)
            };
            DungeonRoomManager d2 = DungeonGenerator.CreateRoomManager(dParams);

            //d.dimensions = new GameEngine.Vector2Int(5, 5);
            Console.WriteLine(d2.DebugString());

            if (args.Count > 0 && args[0][0] == 'r')
            {
                args.RemoveAt(0);
                while (Console.ReadLine().Length == 0)
                    DungeonTest(args);
            }
        }

        public void WeaponTest(List<string> args)
        {
            ItemWeaponGenerationParameters properties = null;

            if (args.Count == 0)
                return;

            switch (args[0])
            {
                case "s": properties = ItemWeaponGenerationPresets.StartWeaponItem; break;
                case "f": properties = ItemWeaponGenerationPresets.RandomWeaponItem; break;
                case "b": properties = ItemWeaponGenerationPresets.BrokenWeaponItem; break;
                case "l": properties = ItemWeaponGenerationPresets.LowQualityWeaponChestItem; break;
                case "m": properties = ItemWeaponGenerationPresets.MidQualityWeaponChestItem; break;
                case "h": properties = ItemWeaponGenerationPresets.HighQualityWeaponChestItem; break;
                case "c":
                    if (args.Count > 1 && int.TryParse(args[1], out int level))
                        properties = ItemWeaponGenerationPresets.GenerateWeaponAtLevel(level, true, false);
                    else { Console.WriteLine("Invalid level argument."); return; }
                    break;
            }

            var weapon = DungeonGenerator.GenerateWeapon(properties);
            Console.WriteLine(weapon.DebugString(string.Empty, 0));

            if (args.Count > 0 && args[args.Count - 1][0] == 'r')
            {
                args.RemoveAt(args.Count - 1);
                while (Console.ReadLine().Length == 0)
                    WeaponTest(args);
            }
        }

        public void CreatureTest(List<string> args)
        {
            CreatureGenerationParameters cParams = new CreatureGenerationParameters()
            {
                BaseHealthRange = new Vector2Int(0, 0),
                BaseStatRange = new Vector2Int(0, 0),
                LevelRange = new Vector2Int(1, 1),
                WeaponChance = 1.0f
            };

            Creature creature = DungeonGenerator.GenerateCreature(cParams);
            Console.WriteLine(creature.DebugString(string.Empty, 0));

            if (args.Count > 0 && args[args.Count - 1][0] == 'r')
            {
                args.RemoveAt(args.Count - 1);
                while (Console.ReadLine().Length == 0)
                    CreatureTest(args);
            }
        }

        public void ChestTest(List<string> args)
        {
            int level = 6;
            int itemCountMin = 10;
            int itemCountMax = itemCountMin;

            if (args.Count < 1 || !int.TryParse(args[0], out level))
                Console.WriteLine($"Invalid or empty level argument, defaulting to Level {level}.");
            if (args.Count < 2 || !int.TryParse(args[1], out itemCountMin))
                Console.WriteLine($"Invalid or empty minItemCount argument, defaulting to {itemCountMin}.");
            if (args.Count < 3 || !int.TryParse(args[2], out itemCountMax))
                Console.WriteLine($"Invalid or empty maxItemCount argument, defaulting to {itemCountMin}.");

            while (BaseRepeatableCommand(args.Count - 1, args))
            {
                DungeonChestGenerationParamerters cParams = new DungeonChestGenerationParamerters(2, () => EnumExt<QualityLevel>.RandomValue)
                {
                    CreatureLevel = 6,
                    ChestType = DungeonChestType.Weapon,
                    ItemRange = new Vector2Int(itemCountMin, itemCountMax),
                };

                var chest = DungeonGenerator.GenerateChest(cParams);
                Console.WriteLine(chest.DebugString(string.Empty, 0));
            }
        }

        public void MapTest(List<string> args)
        {
            int numMoves = 100;
            PlayerCharacter player = crawlerGame.player;
            Dungeon dungeon = crawlerGame.GenerateDungeon(DungeonSize.Huge);

            DungeonRoom GetDungeonRoom(int index) => dungeon.roomManager.rooms[index];
            DungeonRoom GetRandomConnectedRoom(DungeonRoom room, DungeonRoom prev)
            {
                if (room.connections.Count == 1 && room.Index != prev.Index)
                    return prev;
                return dungeon.roomManager.rooms[room.connections.Keys.RandomItem((i) => i != prev.Index)];
            }

            void MarkConnectionsChecked(DungeonRoom room)
            {
                foreach (var kvp in room.connections)
                {
                    dungeon.PlayerCheckRoom(player, GetDungeonRoom(kvp.Key));
                }
            }

            dungeon.MovePlayerToRoom(player, dungeon.roomManager.EntranceRoom);
            MarkConnectionsChecked(player.CurrentRoom);

            // Simulate navigating the dungeon
            //DungeonRoom pRoom = player.CurrentRoom;
            //for (int i = 0; i < numMoves; ++i)
            //{
            //    DungeonRoom rRoom = GetRandomConnectedRoom(player.CurrentRoom, pRoom);
            //    pRoom = player.CurrentRoom;
            //    dungeon.MovePlayerToRoom(player, rRoom);
            //    MarkConnectionsChecked(rRoom);
            //}
            foreach (var room in dungeon.roomManager.rooms)
                dungeon.MovePlayerToRoom(player, room);

            Console.WriteLine(dungeon.roomManager.DebugString());
            Console.WriteLine();
            dungeon.BuildPlayerMap(player).WriteLine();
        }

        public void ExpTest(List<string> args)
        {
            if (args.Count > 0 && int.TryParse(args[0], out int result))
            {
                int xp = 0;
                PlayerCharacter player = new PlayerCharacter() { Level = 2 };
                result = Math.Max(player.Level, result);

                for (int i = player.Level; i < result; ++i)
                {
                    xp += player.ExperienceNeeded;
                    player.Level += 1;
                }

                Console.WriteLine($"Experience needed to reach Lv.{result}: {xp}");
            }
        }

        public void NavTest(List<string> args)
        {
            PlayerCharacter player = crawlerGame.player;
            Dungeon dungeon = crawlerGame.GenerateDungeon(DungeonSize.Huge);

            DungeonRoom GetDungeonRoom(int index) => dungeon.roomManager.rooms[index];

            foreach (var room in dungeon.roomManager.rooms)
                dungeon.MovePlayerToRoom(player, room);

            do
            {
                DungeonRoom room2 = dungeon.roomManager.rooms.RandomItem();
                dungeon.MovePlayerToRoom(player, dungeon.roomManager.rooms.RandomItem());

                if (dungeon.CanPlayerPathToRoom(player, room2))
                {
                    dungeon.BuildPlayerMap(player).WriteLine();
                    Console.WriteLine(dungeon.navPath.ToString((room) => room.Index.ToString(), "->"));
                }
                else Console.WriteLine($"Cannot navigate from {player.CurrentRoom.Index} to {room2.Index}");
            } while (BaseRepeatableCommand(args.Count - 1, args));
        }
    }
}
