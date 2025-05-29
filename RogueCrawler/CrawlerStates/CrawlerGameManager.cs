using System;
using CommandEngine;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Transactions;

namespace RogueCrawler
{
    class CrawlerGameManager : BaseCrawlerStateManager
    {
        struct CreatureTurn
        {
            public Creature Creature { get; set; }
            public int MaxActions { get; set; }
            public float Speed => Creature.CombatSpeed.Value;
        }

        //CommandModule commands = new CommandModule("\nEnter next game command");
        CommandModule<int> commands = new CommandModule<int>("\nEnter next game command");

        int turnIndex = 0;
        bool newRoom = false;
        bool dungeonExit = false;
        string firstDungeonMessage = "Entering first dungeon:";
        SmartStringBuilder staticBuilder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);
        ColorStringBuilder colorBuilder = new ColorStringBuilder(DungeonCrawlerSettings.TabString);
        List<CreatureTurn> creatureTurnOrder = new List<CreatureTurn>();

        CreatureTurn CurrentTurn => creatureTurnOrder[turnIndex];
        CreatureTurn NextTurn => turnIndex < creatureTurnOrder.LastIndex()
            ? creatureTurnOrder[turnIndex + 1]
            : creatureTurnOrder[0];

        public CrawlerGameManager(DungeonCrawlerManager manager) : base(manager)
        {
            // Free Commands
            commands.Add(new ConsoleCommand<int>("check", LookToRoom));
            commands.Add(new ConsoleCommand<int>("creature", InspectCreature));
            commands.Add(new ConsoleCommand<int>("cr", InspectCreature));
            commands.Add(new ConsoleCommand<int>("item", InspectItem));
            commands.Add(new ConsoleCommand<int>("inventory", Inventory));
            commands.Add(new ConsoleCommand<int>("inv", Inventory));
            commands.Add(new ConsoleCommand<int>("map", PrintPlayerMap));
            commands.Add(new ConsoleCommand<int>("path", PathToRoom));
            commands.Add(new ConsoleCommand<int>("self", CheckSelf));

            // Action Commands
            commands.Add(new ConsoleCommand<int>("chest", InspectChest));
            commands.Add(new ConsoleCommand<int>("equip", Equip));
            commands.Add(new ConsoleCommand<int>("exit", ExitDungeon));
            commands.Add(new ConsoleCommand<int>("fight", FightCreature));
            commands.Add(new ConsoleCommand<int>("move", MoveToRoom));
            commands.Add(new ConsoleCommand<int>("take", TakeItem));

            // No Creature Commands
            commands.Add(new ConsoleCommand<int>("rest", Rest));
            commands.Add(new ConsoleCommand<int>("takeall", TakeAllItems));
        }

        public override void StartCrawlerState()
        {
            DungeonSize size = (DungeonSize)Math.Min(EnumExt<DungeonSize>.Count - 1, player.Level / DungeonCrawlerSettings.LevelsPerDungeonSizeUnlock);
            dungeon = crawlerManager.GenerateDungeon(size);

            Console.WriteLine(player.InspectString($"Your Stats:\n  Name: {player.ObjectName}", 0));
            Console.WriteLine("\nEnter To Continue...");
            Console.ReadLine();
            Console.WriteLine(dungeon.InspectString($"Entering {size} dungeon...", 0));
            Console.WriteLine();

            dungeonExit = false;
            player.ResetDungeonStats();
            player.HealAllStatsAndAfflictions();
            player.CurrentRoom = dungeon.roomManager.EntranceRoom; // Fix for new character having null as their start room
            PlayerMoveToRoom(dungeon.roomManager.EntranceRoom);
        }
        public override CrawlerState UpdateCrawlerState()
        {
            if(newRoom)
            {
                newRoom = false;
                BuildCreatureTurnOrder();
            }

            if (creatureTurnOrder.Count > 1)
            {
                bool isPlayerDead = false;

                for (turnIndex = 0; turnIndex < creatureTurnOrder.Count; ++turnIndex)
                {
                    Creature creature = CurrentTurn.Creature;

                    // Remove dead creatures from combat.
                    if (!creature.IsAlive)
                    {
                        creatureTurnOrder.RemoveAt(turnIndex--);
                        continue;
                    }

                    // Take all creature turns.
                    for (int i = 0; i < CurrentTurn.MaxActions && !isPlayerDead; ++i)
                    {
                        if (creature.ID != player.ID)
                        {
                            // Sleep here to prevent dumping all creature turns at once.
                            System.Threading.Thread.Sleep(500);
                            isPlayerDead = TakeCreatureTurn(creature);
                        }
                        else
                        {
                            commands.NextCommand(true, out int actionPoints);
                            i += actionPoints - 1;
                        }
                    }

                    if (isPlayerDead)
                    {
                        OnPlayerDeath();
                        dungeonExit = true;
                        break;
                    }
                }
            }
            else commands.NextCommand(true, out int actionPoints);

            if (ExitGameState())
                return CrawlerState.Menu;
            return CrawlerState.Game;
        }
        public override void EndCrawlerState() { colorBuilder.Clear(); }

        private void PlayerMoveToRoom(DungeonRoom room)
        {
            newRoom = true;
            dungeon.MovePlayerToRoom(player, room);

            int tabCount = 0;
            staticBuilder.Clear();
            staticBuilder.NewlineAppend(tabCount, dungeon.InspectRoomString(room, $"Moved to Room {room.Index}:", tabCount));
            Console.WriteLine(staticBuilder.ToString());
        }
        private void PlayerCheckRoom(DungeonRoom room)
        {
            dungeon.PlayerCheckRoom(player, room);
            Console.WriteLine(dungeon.CheckRoomString(room, string.Empty, 0));
        }
        private void PlayerInspectRoom(DungeonRoom room)
        {
            string prefix = string.Empty;

            if (player.CurrentRoom.Index == room.Index)
                prefix = "Inside the room:";
            else prefix = $"Inside Room {room.Index}: ";

            Console.WriteLine(dungeon.InspectRoomString(room, prefix, 0));
        }

        private void PlayerUpdateFatigue(int tabCount, float amount)
        {
            player.Fatigue.AddValue(amount);

            ConsoleColor percentColor = ConsoleColor.Red;

            if (player.Fatigue.Percent > 0.66f)
                percentColor = ConsoleColor.Green;
            else if (player.Fatigue.Percent > 0.33f)
                percentColor = ConsoleColor.Yellow;

            colorBuilder.Clear();
            colorBuilder.Append(tabCount, $" Current ");
            colorBuilder.Append("Fatigue", ConsoleColor.Green);
            colorBuilder.Append(": ", ConsoleColor.Gray);
            colorBuilder.Append(player.Fatigue.Value.ToString(), percentColor);
            colorBuilder.AppendLine(player.Fatigue.MaxValue.ToString(), ConsoleColor.Green);
        }

        private int PlayerSellLootMenu(int tabCount, out int soldItemCount)
        {
            if (player.Inventory.ItemCount == 0)
            {
                soldItemCount = 0;
                return 0;
            }

            staticBuilder.Clear();
            staticBuilder.Append(tabCount, "Exiting dungeon and returning to town.");
            ++tabCount;
            staticBuilder.NewlineAppend(tabCount, "Select the loot inputting their IDs.");
            staticBuilder.NewlineAppend(tabCount, "You can put as many IDs per command input as you want.");
            staticBuilder.NewlineAppend(tabCount, "Exit this menu by entering \'exit\', or by entering an empty command.");
            --tabCount;

            staticBuilder.NewlineAppend(player.Inventory.InspectString("Please select what loot you DO NOT want to sell:", tabCount));
            staticBuilder.AppendLine(string.Empty);

            Console.WriteLine(staticBuilder.ToString());

            string input = CommandManager.UserInputPrompt("IDs", false);
            string[] splitInput = input.Split(' ');
            List<string> args = new List<string>(1) { string.Empty };
            List<(IItem item, int count)> saveItems = new List<(IItem item, int count)>();

            while (splitInput[0].Length > 0 && splitInput[0] != "exit")
            {
                foreach (string s in splitInput)
                {
                    args[0] = s;
                    if (BaseInventoryItemCommand(args, out IItem item, out string errorMsg))
                    {
                        if (player.Inventory.RemoveAllItems(item.ID, out (IItem item, int count) itemData))
                            saveItems.Add(itemData);
                    }
                    else if (s == "exit")
                    {
                        splitInput[0] = s;
                        break;
                    }
                }

                input = CommandManager.UserInputPrompt("IDs", false);
                splitInput = input.Split(' ');
            }

            int lootExp = 0;
            soldItemCount = 0;
            foreach (var item in player.Inventory.RemoveAllItems())
            {
                ++soldItemCount;
                lootExp += item.GetValue();
            }

            foreach (var itemData in saveItems)
                player.Inventory.AddItem(itemData.item, itemData.count);

            return lootExp;
        }
        private void PlayerExperienceMenu(int tabCount, params int[] expSources)
        {
            staticBuilder.Clear();
            staticBuilder.Append(tabCount, $"Experience break down:");
            tabCount++;
            staticBuilder.NewlineAppend(tabCount, $"Exploration: {player.ExploredRooms.Count} * {DungeonCrawlerSettings.ExperiencePerExploredRoom}");
            if (player.ExploredRooms.Count >= dungeon.roomManager.rooms.Count)
            {
                expSources[0] = (int)(expSources[0] * DungeonCrawlerSettings.FullExploreBonus);
                staticBuilder.Append($" x [FULL MAP BONUS]");
            }
            staticBuilder.Append($" = {expSources[0]}");

            staticBuilder.NewlineAppend(tabCount, $"Kills: {player.CreaturesKilled} * {DungeonCrawlerSettings.ExperiencePerCreatureKilled}");
            if (player.CreaturesKilled >= dungeon.CreatureCount)
            {
                expSources[1] = (int)(expSources[1] * DungeonCrawlerSettings.FullClearBonus);
                staticBuilder.Append($" x [FULL CLEAR BONUS]");
            }
            staticBuilder.Append($" = {expSources[1]}");

            staticBuilder.NewlineAppend(tabCount, $"Loot: {expSources[3]} item sold for {expSources[2]}");
            tabCount--;

            int totalExp = expSources[0] + expSources[1] + expSources[2];
            player.Experience += totalExp;
            staticBuilder.NewlineAppend(tabCount, $"GRAND TOTAL: {totalExp}exp");
            Console.WriteLine(staticBuilder.ToString());
        }

        private bool ExitGameState()
        {
            return dungeonExit;
        }

        private bool TakeCreatureTurn(Creature c)
        {
            float damage = c.GetCombatDamage();
            bool playerDied = dungeon.DamageCreature(player, c.GetCombatDamage(), out float rDamage);

            staticBuilder.Clear();
            staticBuilder.NewlineAppend($"{c.ToString()} attacks for {rDamage} damage!");
            staticBuilder.NewlineAppend($"HP Left: {player.Health.Value}/{player.Health.MaxValue}");
            Console.WriteLine(staticBuilder.ToString());

            return playerDied;
        }
        private void OnPlayerDeath()
        {
            staticBuilder.Clear();
            staticBuilder.NewlineAppend(1, "----------  YOU DIED  ----------");
            staticBuilder.NewlineAppend(1, "Enter 'newGame' for a new game.");
            Console.WriteLine(staticBuilder.ToString());
        }

        private void BuildCreatureTurnOrder()
        {
            creatureTurnOrder.Clear();

            // Sort combatants by combat speed;
            if (dungeon.creatureManager.GetObjectCount(player.CurrentRoom) > 0)
            {
                int playerIndex = 0;
                var roomCreatures = dungeon.creatureManager.GetObjectsInRoom(player.CurrentRoom);
                roomCreatures.Add(player);
                roomCreatures.Sort((a, b) => b.CombatSpeed.Value.CompareTo(a.CombatSpeed.Value));

                foreach (var creature in roomCreatures)
                {
                    float speed = creature.CombatSpeed.Value;
                    int actions = (int)MathF.Floor(speed);

                    if (CommandEngine.Random.NextFloat() < speed % 1)
                        ++actions;

                    if (creature == player)
                        playerIndex = creatureTurnOrder.Count;

                    creatureTurnOrder.Add(
                        new CreatureTurn()
                        {
                            Creature = creature,
                            MaxActions = actions
                        }
                    );
                }

                // Player always wins speed ties
                int newIndex = playerIndex;
                while (newIndex > 0 && creatureTurnOrder[newIndex - 1].Speed <= player.CombatSpeed.Value)
                    --newIndex;
                if(newIndex < playerIndex)
                {
                    CreatureTurn t = creatureTurnOrder[newIndex];
                    creatureTurnOrder[newIndex] = creatureTurnOrder[playerIndex];
                    creatureTurnOrder[playerIndex] = t;
                }

                PrintEnteredCombatMessage();
            }
            else
            {
                creatureTurnOrder.Add(new CreatureTurn()
                {
                    Creature = player,
                    MaxActions = 1
                });
            }
        }
        private void PrintEnteredCombatMessage()
        {
            int tabCount = 0;
            staticBuilder.Clear();
            staticBuilder.NewlineAppend(tabCount, "You've entered combat! Turn order: ");

            ++tabCount;
            foreach (var creatureTurn in creatureTurnOrder)
                staticBuilder.NewlineAppend(tabCount, $"{creatureTurn.Creature.BriefString()} x{creatureTurn.MaxActions}");
            --tabCount;
            staticBuilder.NewlineAppend(tabCount, "\nPress Enter to continue");
            Console.WriteLine(staticBuilder.ToString());
            Console.ReadLine();
        }

        #region Free Commands
        private bool LookToRoom(List<string> args, out int ap)
        {
            if (args.Count == 0)
            {
                PlayerInspectRoom(player.CurrentRoom);
            }
            else if (BaseNoCreatureCommand(out string errorMsg))
            {
                if (args[0] == "all")
                    foreach (var connection in player.CurrentRoom.connections.Values)
                    {
                        DungeonRoom r = dungeon.roomManager.GetRoomByIndex(connection.index);
                        PlayerCheckRoom(r);
                    }
                else if (BaseRoomCommand(args, out DungeonRoom room, out errorMsg))
                    PlayerCheckRoom(room);
            }
            else Console.WriteLine(errorMsg);

            ap = 0;
            return true;
        }

        private bool InspectCreature(List<string> args, out int ap)
        {
            if (BaseCreatureCommand(args, out var creature, out string errorMsg))
            {
                Console.WriteLine(creature.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(errorMsg);

            ap = 0;
            return true;
        }

        private bool InspectItem(List<string> args, out int ap)
        {
            if (BaseItemCommand(args, out IItem item, out string msg))
            {
                Console.WriteLine(item.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(msg);

            ap = 0;
            return true;
        }

        public bool PrintPlayerMap(List<string> args, out int ap)
        {
            var builder = dungeon.BuildPlayerMap(player);
            builder.WriteLine();

            ap = 0;
            return true;
        }

        public bool Inventory(List<string> args, out int ap)
        {
            ap = 0;

            if (args.Count == 0)
            {
                string str = player.Inventory.InspectString("Items in Inventory:", 0);
                Console.WriteLine(str);
                return true;
            }
            else if (BaseInventoryItemCommand(args, out IItem item, out string errorMsg))
            {
                Console.WriteLine(item.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(errorMsg);

            return true;
        }

        public bool CheckSelf(List<string> args, out int ap)
        {
            ap = 0;
            if (args.Count > 0)
            {
                switch (args[0])
                {
                    case "f":
                    case "full":
                        Console.WriteLine(player.InspectString(string.Empty, 0));
                        return true;
                    case "a":
                    case "attr":
                        Console.WriteLine(player.MaxAttributes.InspectString("Your Attributes:", 0));
                        return true;
                    case "w":
                    case "weapon":
                        Console.WriteLine(player.PrimaryWeapon.InspectString("Your Weapon Stats:", 0));
                        return true;
                }
            }

            Console.WriteLine(player.BriefInspectString(string.Empty, 0));
            return true;
        }

        public bool PathToRoom(List<string> args, out int ap)
        {
            if (BaseRoomCommand(args, out DungeonRoom room, out string errorMsg))
            {
                if (player.CurrentRoom.ConnectedTo(room))
                    PlayerMoveToRoom(room);
                else if (player.RoomIsExplorable(room))
                {
                    if (dungeon.CanPlayerPathToRoom(player, room))
                    {
                        staticBuilder.Clear();
                        staticBuilder.Append($"Path to {room.Index}:");
                        staticBuilder.NewlineAppend(1, dungeon.navPath.ToString((room) => room.Index.ToString(), "->"));
                        staticBuilder.NewlineAppend(1, "(Print the map to see the path)");
                        Console.WriteLine(staticBuilder.ToString());
                    }
                    else
                    {
                        Console.WriteLine($"No path to room {room.Index}.");
                        dungeon.ClearPath();
                    }
                }
                else Console.WriteLine("You don't know where that room is.");
            }
            else if (args.Count > 0 && args[0] == "clear")
            {
                DungeonRoom endRoom = dungeon.navPath.Last();
                dungeon.ClearPath();
                Console.WriteLine($"Cleared path to room {endRoom.Index} from the map.");
            }

            ap = 0;
            return true;
        }
        #endregion

        #region Action Commands
        public bool Equip(List<string> args, out int ap)
        {
            if (BaseInventoryItemCommand(args, out IItem item, out string errorMsg))
            {
                ItemWeapon weapon = item as ItemWeapon;

                ap = 1;
                if (weapon == null)
                {
                    Console.WriteLine("Item is not a weapon!");
                    return true;
                }
                if (!player.CanEquipWeapon(weapon))
                {
                    Console.WriteLine("You dont meet the attribute requirements.");
                    return true;
                }
                if (player.Inventory.RemoveItem(item.ID, out item))
                {
                    player.Inventory.AddItem(player.PrimaryWeapon);
                    player.PrimaryWeapon = weapon;
                    Console.WriteLine(weapon.InspectString("Equipped Weapon:", 0));
                    return true;
                }
                else Console.WriteLine("ERROR: failed to find weapon in inventory. This'll be a nasty debug.");
            }
            else Console.WriteLine(errorMsg);

            ap = 0;
            return true;
        }

        private bool TakeItem(List<string> args, out int ap)
        {
            ap = 0;
            if (BaseItemCommand(args, out IItem item, out DungeonChest<IItem> chest, out string errorMsg))
            {
                chest.RemoveItem(item.ID, out item);
                player.Inventory.AddItem(item, 1);
                ap = 1;
            }
            else Console.WriteLine(errorMsg);

            return true; 
        }

        private bool InspectChest(List<string> args, out int ap)
        {
            ap = 0;
            if (BaseChestCommand(args, out var chest, out string errorMsg))
            {
                ap = 1;
                chest.MarkInspected();
                Console.WriteLine(chest.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(errorMsg);

            return true;
        }

        private bool FightCreature(List<string> args, out int ap)
        {
            ap = 0;
            if (BaseCreatureCommand(args, out var creature, out string errorMsg))
            {
                ap = 1;
                player.Fatigue.AddValue(-player.GetAttackFatigueCost());
                if (dungeon.DamageCreature(creature, player.GetCombatDamage(), out float rDamage))
                {
                    ++player.CreaturesKilled;
                    Console.WriteLine($"{creature.ObjectName} died!");
                }
                else Console.WriteLine($"{creature.ObjectName} HP Left: {creature.Health.Value}");
            }
            else Console.WriteLine(errorMsg);

            return true;
        }

        private bool MoveToRoom(List<string> args, out int ap)
        {
            ap = 0;
            if (BaseRoomCommand(args, out DungeonRoom room, out string errorMsg))
            {
                if (!player.CurrentRoom.ConnectedTo(room))
                {
                    errorMsg = "No connection to this room!";
                    Console.WriteLine(errorMsg);
                    return false;
                }

                ap = (int)MathF.Ceiling(player.CombatSpeed.MaxValue);
                PlayerMoveToRoom(room);
            }
            else Console.WriteLine(errorMsg);

            return true;
        }

        public bool ExitDungeon(List<string> args, out int ap)
        {
            if(!BaseNoCreatureCommand(out string msg))
            {
                ap = 0;
                Console.WriteLine(msg);
                return true;
            }

            int tabCount = 0;
            int roomExp = player.ExploredRooms.Count * DungeonCrawlerSettings.ExperiencePerExploredRoom;
            int killExp = player.CreaturesKilled * DungeonCrawlerSettings.ExperiencePerCreatureKilled;
            int lootExp = PlayerSellLootMenu(tabCount, out int soldItemCount);
            ap = (int)MathF.Ceiling(player.CombatSpeed.MaxValue);

            PlayerExperienceMenu(tabCount, roomExp, killExp, lootExp, soldItemCount);

            int levelsGained = player.Level;
            int expNeeded = player.ExperienceNeeded;
            while (player.Experience > expNeeded)
            {
                ++player.Level;
                player.Experience -= expNeeded;
                expNeeded = player.ExperienceNeeded;
            }
            levelsGained = player.Level - levelsGained;
            if (levelsGained > 0)
            {
                int attrPoints = levelsGained * DungeonCrawlerSettings.AttributePointsPerCreatureLevel;
                // attrPoints += player.MaxAttributes.CreatureLevel;
                CharacterCreator.AttributePrompt(player, levelsGained, attrPoints, tabCount);
            }

            dungeonExit = true;
            return true;
        }
        #endregion

        #region NoCreature Commands
        private bool Rest(List<string> args, out int ap)
        {
            if (BaseNoCreatureCommand(out string errorMsg) && BaseIntCommand(args, out int hitPoints, out errorMsg))
            {
                player.Health.AddValue(hitPoints);
                player.Mana.AddValue(hitPoints * 2);
                player.Fatigue.AddValue(hitPoints * 3);

                dungeon.HealAllCreatures(hitPoints);
                Console.WriteLine("You've rested up, but so has the dungeon...");
            }
            else Console.WriteLine(errorMsg);

            ap = 0;
            return true;
        }

        private bool TakeAllItems(List<string> args, out int ap)
        {
            if(!BaseNoCreatureCommand(out string errorMsg))
            {
                Console.WriteLine(errorMsg);
                ap = 0;
                return true;
            }

            DungeonRoom room = player.CurrentRoom;

            if (dungeon.chestManager.GetObjectCount(room) == 0)
                Console.WriteLine("There aren't any objects in the room.");

            int tabCount = 0;
            staticBuilder.Clear();
            staticBuilder.Append(tabCount, "Added following items to Inventory:");

            tabCount++;
            foreach (var chest in dungeon.chestManager.GetObjectsInRoom(room))
            {
                foreach (var item in chest.RemoveAllItems())
                {
                    player.Inventory.AddItem(item);
                    staticBuilder.NewlineAppend(tabCount, item.BriefString());
                }
                chest.MarkInspected();
            }
            tabCount--;
            Console.WriteLine(staticBuilder.ToString());

            ap = 0;
            return true;
        }
        #endregion
    }
}
