using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameEngine;

namespace DnD_Generator
{
    enum GameState { Menu, Game }
    class DungeonCrawlerManager
    {
        public Dungeon dungeon;
        public PlayerCharacter player;

        public static DungeonCrawlerManager Instance { get; private set; }
        public static int PlayerLevel { get => Instance?.player.Level ?? 0; }

        int fightCommands = 0;
        bool dungeonTurn = true;
        GameState gameState = GameState.Menu;
        public Dictionary<string, ConsoleCommand> gameCommands = new Dictionary<string, ConsoleCommand>();
        public Dictionary<string, ConsoleCommand> menuCommands = new Dictionary<string, ConsoleCommand>();

        public DungeonCrawlerManager()
        {
            Instance = this;
            menuCommands.Add(ConsoleCommand.Create("newGame", NewGame));
            menuCommands.Add(ConsoleCommand.Create("again", NewDungeon));

            gameCommands.Add(ConsoleCommand.Create("check", LookToRoom));
            gameCommands.Add(ConsoleCommand.Create("chest", InspectChest));
            gameCommands.Add(ConsoleCommand.Create("creature", InspectCreature));
            gameCommands.Add(ConsoleCommand.Create("cr", InspectCreature));
            gameCommands.Add(ConsoleCommand.Create("equip", Equip));
            gameCommands.Add(ConsoleCommand.Create("exit", ExitDungeon));
            gameCommands.Add(ConsoleCommand.Create("fight", FightCreature));
            gameCommands.Add(ConsoleCommand.Create("item", InspectItem));
            gameCommands.Add(ConsoleCommand.Create("inventory", Inventory));
            gameCommands.Add(ConsoleCommand.Create("inv", Inventory));
            gameCommands.Add(ConsoleCommand.Create("map", PrintPlayerMap));
            gameCommands.Add(ConsoleCommand.Create("move", MoveToRoom));
            gameCommands.Add(ConsoleCommand.Create("rest", Rest));
            gameCommands.Add(ConsoleCommand.Create("take", TakeItem));
            gameCommands.Add(ConsoleCommand.Create("takeall", TakeAllItems));
            gameCommands.Add(ConsoleCommand.Create("self", CheckSelf));

            player = new PlayerCharacter();
        }

        public void UpdateLoop()
        {
            switch(gameState)
            {
                case GameState.Menu:
                    CommandManager.GetNextCommand("\nEnter next menu command", true, menuCommands);
                    break;
                case GameState.Game:
                    CommandManager.GetNextCommand("\nEnter next game command", true, gameCommands);
                    if (dungeonTurn)
                        DungeonTurn();
                    break;
            }
        }

        #region Menu Commands
        public void NewGame(List<string> args)
        {
            dungeonTurn = false;
            player = CharacterCreator.CharacterCreationPrompt();
            StartDungeon(DungeonSize.Small, "Entering first dungeon:");
        }

        public void NewDungeon(List<string> args)
        {
            if(dungeonTurn)
            {
                Console.WriteLine("Cannot enter a new dungeon, YOU'RE DEAD.");
                return;
            }

            DungeonSize size = (DungeonSize)Math.Min(EnumExt<DungeonSize>.Count - 1, player.Level / DungeonCrawlerSettings.LevelsPerDungeonSizeUnlock);
            StartDungeon(size, $"Entering {size} dungeon...");
        }
        #endregion

        #region Game Commands
        private void LookToRoom(List<string> args)
        {
            if (args.Count == 0)
            {
                PlayerInspectRoom(player.CurrentRoom);
            }
            else if (BaseNoCreatureCommand(out string errorMsg) && BaseRoomCommand(args, out DungeonRoom room, out errorMsg))
            {
                PlayerCheckRoom(room);
            }
            else Console.WriteLine(errorMsg);
        }

        private void MoveToRoom(List<string> args)
        {
            if (BaseRoomCommand(args, out DungeonRoom room, out string errorMsg) && player.CurrentRoom.ConnectedTo(room))
            {
                PlayerMoveToRoom(room);
            }
            else Console.WriteLine(errorMsg);
        }

        private void Rest(List<string> args)
        {
            if (BaseNoCreatureCommand(out string errorMsg) && BaseIntCommand(args, out int hitPoints, out errorMsg))
            {
                float maxHp = player.MaxHitPoints;
                float gainedHp = Mathc.Min(hitPoints, maxHp - player.HitPoints);
                player.HitPoints = Mathc.Min(player.HitPoints + gainedHp, maxHp);
                dungeon.HealAllCreatures(gainedHp);
                Console.WriteLine("You've rested up, but so has the dungeon...");
            }
            else Console.WriteLine(errorMsg);
        }

        private void FightCreature(List<string> args)
        {
            if(BaseCreatureCommand(args, out var creature, out string errorMsg))
            {
                ++fightCommands;
                if (dungeon.DamageCreature(creature, player.Damage))
                {
                    ++player.CreaturesKilled;
                    Console.WriteLine($"{creature.Name} died!");
                }

                dungeonTurn |= fightCommands % DungeonCrawlerSettings.CommandsPerCreatureAttack == 0;
            }
            else Console.WriteLine(errorMsg);
        }

        private void InspectCreature(List<string> args)
        {
            if (BaseCreatureCommand(args, out var creature, out string errorMsg))
            {
                Console.WriteLine(creature.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(errorMsg);
        }

        private void InspectChest(List<string> args)
        {
            if (BaseChestCommand(args, out var chest, out string errorMsg))
            {
                chest.MarkInspected();
                Console.WriteLine(chest.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(errorMsg);
        }

        private void InspectItem(List<string> args)
        {
            if(BaseItemCommand(args, out IItem item, out string msg))
            {
                Console.WriteLine(item.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(msg);
        }

        private void TakeItem(List<string> args)
        {
            if (BaseItemCommand(args, out IItem item, out DungeonChest<IItem> chest, out string errorMsg))
            {
                chest.RemoveItem(item.ID, out item);
                player.Inventory.AddItem(item, 1);
            }
            else Console.WriteLine(errorMsg);
        }

        private void TakeAllItems(List<string> args)
        {
            DungeonRoom room = player.CurrentRoom;

            if (dungeon.chestManager.GetObjectCount(room) == 0)
                Console.WriteLine("There aren't any objects in the room.");

            int tabCount = 0;
            staticBuilder.Clear();
            staticBuilder.Append(tabCount, "Added following items to Inventory:");

            tabCount++;
            foreach(var chest in dungeon.chestManager.GetObjectsInRoom(room))
            {
                foreach(var item in chest.RemoveAllItems())
                {
                    player.Inventory.AddItem(item);
                    staticBuilder.NewlineAppend(tabCount, item.BriefString());
                }
                chest.MarkInspected();
            }
            tabCount--;
            Console.WriteLine(staticBuilder.ToString());
        }

        public void PrintPlayerMap(List<string> args)
        {
            var builder = dungeon.BuildPlayerMap(player);
            builder.WriteLine();
        }

        public void Inventory(List<string> args)
        {
            if (args.Count == 0)
            {
                string str = player.Inventory.InspectString("Items in Inventory:", 0);
                Console.WriteLine(str);
                return;
            }
            else if (BaseInventoryItemCommand(args, out IItem item, out string errorMsg))
            {
                Console.WriteLine(item.InspectString(string.Empty, 0));
            }
            else Console.WriteLine(errorMsg);
        }

        public void Equip(List<string> args)
        {
            if (BaseInventoryItemCommand(args, out IItem item, out string errorMsg))
            {
                ItemWeapon weapon = item as ItemWeapon;
                
                if(weapon == null)
                {
                    Console.WriteLine("Item is not a weapon!");
                    return;
                }
                if (!weapon.CanEquip(player.Attributes))
                {
                    Console.WriteLine("You dont meet the attribute requirements.");
                    return;
                }
                if (player.Inventory.RemoveItem(item.ID, out item))
                {
                    player.Inventory.AddItem(player.PrimaryWeapon);
                    player.PrimaryWeapon = weapon;
                    Console.WriteLine(weapon.InspectString("Equipped Weapon:", 0));
                    return;
                }
                else Console.WriteLine("ERROR: failed to find weapon in inventory. This'll be a nasty debug.");
            }
            else Console.WriteLine(errorMsg);
        }

        public void ExitDungeon(List<string> args)
        {
            int tabCount = 0;
            int roomExp = player.ExploredRooms.Count * DungeonCrawlerSettings.ExperiencePerExploredRoom;
            int killExp = player.CreaturesKilled * DungeonCrawlerSettings.ExperiencePerCreatureKilled;
            int lootExp = PlayerSellLootMenu(tabCount, out int soldItemCount);

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
            if(levelsGained > 0)
            {
                int attrPoints = levelsGained * DungeonCrawlerSettings.AttributePointsPerLevel;
                attrPoints += CreatureAttributes.GetMissingPoints(player.Attributes);
                CharacterCreator.AttributePrompt(player, levelsGained, attrPoints, tabCount);
            }
            gameState = GameState.Menu;
        }

        public void CheckSelf(List<string> args)
        {
            if(args.Count > 0)
            {
                switch (args[0])
                {
                    case "full":
                        Console.WriteLine(player.InspectString(string.Empty, 0));
                        return;
                    case "attr":
                        Console.WriteLine(player.Attributes.InspectString("Your Attributes:", 0));
                        return;
                    case "weapon":
                        Console.WriteLine(player.PrimaryWeapon.InspectString("Your Weapon Stats:", 0));
                        return;
                }
            }
            Console.WriteLine(player.BriefInspectString(string.Empty, 0));
        }

        public void PathToRoom(List<string> args)
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
                Console.WriteLine($"Cleared path to room {room.Index} from the map.");
            }
        }
        #endregion

        #region Console Print and Surface Logic
        SmartStringBuilder staticBuilder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

        private void PlayerMoveToRoom(DungeonRoom room)
        {
            if (DungeonTurn())
                return;

            dungeon.MovePlayerToRoom(player, room);
            ResetOnMove();

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

        private int PlayerSellLootMenu(int tabCount, out int soldItemCount)
        {
            if(player.Inventory.ItemCount == 0)
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
            List<IItem> saveItems = new List<IItem>();

            while (splitInput[0].Length > 0 && splitInput[0] != "exit")
            {
                foreach(string s in splitInput)
                {
                    args[0] = s;
                    if (BaseInventoryItemCommand(args, out IItem item, out string errorMsg))
                    {
                        if (player.Inventory.RemoveAllItems(item.ID, out (IItem item, int count) itemData))
                            for (int i = 0; i < itemData.count; ++i)
                                saveItems.Add(item);
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
            foreach(var item in player.Inventory.RemoveAllItems())
            {
                ++soldItemCount;
                lootExp += item.GetValue();
            }

            foreach (var item in saveItems)
                player.Inventory.AddItem(item);

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

        private void StartDungeon(DungeonSize size, string dungeonPrefix)
        {
            Console.WriteLine(player.InspectString($"Your Stats:\n  Name: {player.Name}", 0));
            Console.WriteLine("\nEnter To Continue...");
            Console.ReadLine();

            dungeon = GenerateDungeon(size);

            Console.WriteLine(dungeon.InspectString(dungeonPrefix, 0));
            Console.WriteLine();

            player.ResetDungeonStats();
            player.CurrentRoom = dungeon.roomManager.EntranceRoom; // Fix for new character having null as their start room
            PlayerMoveToRoom(dungeon.roomManager.EntranceRoom);

            gameState = GameState.Game;
        }

        private bool DungeonTurn()
        {
            if(dungeon.creatureManager.GetObjectCount(player.CurrentRoom) > 0)
            {
                Creature c = dungeon.creatureManager.GetRandomObject(player.CurrentRoom);
                float damage = c.Damage;

                staticBuilder.Clear();
                staticBuilder.NewlineAppend($"{c.ToString()} attacks for {damage} damage!");
                
                if(dungeon.DamageCreature(player, c.Damage))
                {
                    staticBuilder.NewlineAppend(1, "----------  YOU DIED  ----------");
                    staticBuilder.NewlineAppend(1, "Enter 'newGame' for a new game.");
                    gameState = GameState.Menu;
                    Console.WriteLine(staticBuilder.ToString());
                    return true;
                }

                staticBuilder.NewlineAppend(1, $"HP Left: {player.HitPoints}/{player.MaxHitPoints}");
                Console.WriteLine(staticBuilder.ToString());
            }
            dungeonTurn = false;
            return false;
        }

        private void ResetOnMove()
        {
            fightCommands = 0;
        }
        #endregion

        #region Base Commands
        private bool BaseCreatureCommand(List<string> args, out Creature creature, out string errorMsg)
        {
            if (BaseIntCommand(args, out int creatureId, out errorMsg))
            {
                if (dungeon.creatureManager.GetObjectCount(player.CurrentRoom) == 0)
                {
                    errorMsg = ("There are no creatures in this room.");
                    creature = null;
                    return false;
                }

                creature = dungeon.creatureManager.GetObjectInRoom(player.CurrentRoom, creatureId);
                if (creature != null)
                {
                    errorMsg = string.Empty;
                    return true;
                }
            }

            creature = null;
            errorMsg += (". Not a valid creature ID");
            return false;
        }
        
        private bool BaseChestCommand(List<string> args, out DungeonChest<IItem> chest, out string errorMsg)
        {
            if (args.Count > 0 && int.TryParse(args[0], out int creatureId))
            {
                if (dungeon.chestManager.GetObjectCount(player.CurrentRoom) == 0)
                {
                    errorMsg = ("There are no chests in this room.");
                    chest = null;
                    return false;
                }

                chest = dungeon.chestManager.GetObjectInRoom(player.CurrentRoom, creatureId);
                if (chest != null)
                {
                    errorMsg = string.Empty;
                    return true;
                }
            }

            chest = null;
            errorMsg = ("Not a valid chest ID.");
            return false;
        }
        
        private bool BaseItemCommand(List<string> args, out IItem item, out DungeonChest<IItem> containingChest, out string errorMsg)
        {
            item = default(IItem);
            containingChest = null;

            if (args.Count > 0 && int.TryParse(args[0], out int itemId))
            {
                var chests = dungeon.chestManager.GetObjectsInRoom(player.CurrentRoom);

                if (chests.Count == 0)
                {
                    errorMsg = ("There are no items in this room.");
                    return false;
                }

                foreach (var chest in chests)
                    if (chest.Inspected && chest.ContainsItem(itemId))
                    {
                        item = chest.GetItem(itemId);
                        containingChest = chest; 
                        errorMsg = string.Empty;
                        return true;
                    }
            }

            errorMsg = ("Not a valid item ID.");
            return false;
        }
        private bool BaseItemCommand(List<string> args, out IItem item, out string errorMsg)
            => BaseItemCommand(args, out item, out DungeonChest<IItem> swallowedRef, out errorMsg);
       
        private bool BaseRoomCommand(List<string> args, out DungeonRoom room, out string errorMsg)
        {
            room = null;
            if (args.Count > 0 && int.TryParse(args[0], out int index))
            {
                room = dungeon.roomManager.GetRoomByIndex(index);
                errorMsg = "Not a valid room index.";
                return room != null;
            }
            errorMsg = ("Not a valid room index.");
            return false;
        }

        private bool BaseInventoryItemCommand(List<string> args, out IItem item, out string errorMsg)
        {
            item = default(IItem);

            if (args.Count > 0 && int.TryParse(args[0], out int itemId))
            {
                var chest = player.Inventory;
                if (chest.ContainsItem(itemId))
                {
                    item = chest.GetItem(itemId);
                    errorMsg = string.Empty;
                    return true;
                }
            }

            errorMsg = ("Not a valid item ID.");
            return false;
        }

        private bool BaseNoCreatureCommand(out string errorMsg)
        {
            errorMsg = "Cannot use this commands while creatures are in the room!";
            return !dungeon.creatureManager.RoomContainsObjects(player.CurrentRoom);
        }

        private bool BaseIntCommand(List<string> args, out int result, out string errorMsg)
        {
            result = 0;
            errorMsg = "Please enter a valid integer";
            if(args.Count > 0 && int.TryParse(args[0], out result))
            {
                return true;
            }
            return false;
        }
        #endregion

        public Dungeon GenerateDungeon(DungeonSize size)
        {
            Vector2Int GetRoomRange()
            {
                switch(size)
                {
                    case DungeonSize.Small: return new Vector2Int(20, 35); // 4x5, 5x5, 5x6, =5x7
                    case DungeonSize.Medium: return new Vector2Int(35, 50); // 5x7, 6x6, 6x7, 6x8, 6x9, 7x7, >7x8
                    case DungeonSize.Large: return new Vector2Int(50, 75); // 7x8, 7x9, 8x8, 8x9, >8x10
                    case DungeonSize.Huge: return new Vector2Int(75, 100); // <8x10, 9x9, 9x10, 9x11, =10x10
                }
                return new Vector2Int(25, 25); // =5x5
            }
            var roomRange = GetRoomRange();
            DungeonGenerationParameters dParams = new DungeonGenerationParameters(3, () => DungeonGenerator.GetShiftedQuality(QualityLevel.Mid))
            {
                PlayerLevel = player.Level,
                RoomRange = roomRange,
                ConnectionRange = new Vector2Int(1, 3),
                RoomHeightRange = new Vector2Int(1, 1),
                RoomWidthRange = new Vector2Int(1, 1),
                MaxCreaturesPerRoom = 3,
                CreatureProbability = 0.33f,
                ChestProbability = 0.2f
            };
            return DungeonGenerator.GenerateDungeon(dParams);
        }
    }
}
