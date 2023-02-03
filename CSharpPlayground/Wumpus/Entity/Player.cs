using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandEngine;

namespace CSharpPlayground.Wumpus
{
    class Player : BoardEntity
    {
        bool invChanged = false;

        EntityCommandModule commandModule;
        Dictionary<ITEM_ID, int> inventory = new Dictionary<ITEM_ID, int>();
        TextBox inventoryWindow;

        public Player(TextBox invWindow)
        {
            inventoryWindow = invWindow;
        }

        public override void Awake()
        {
            commandModule = entity.AddComponent<EntityCommandModule>();
            commandModule.RegisterCommand(new ConsoleCommand("Use", UseCommand));
            commandModule.RegisterCommand(new ConsoleCommand("Peek", PeekCommand));
            commandModule.RegisterCommand(new ConsoleCommand("Move", MoveCommand));
            commandModule.RegisterCommand(new ConsoleCommand("Pickup", PickCommand));
            commandModule.RegisterCommand(new ConsoleCommand("Drop", DropCommand));
        }

        public override void Init(BoardRoom room)
        {
            base.Init(room);
            WumpusGameManager.WriteLine($"Player Spawned at room {CurrentRoom.index}");
        }

        public override void Update()
        {
            if (WumpusGameManager.GetNextCommand(out string command))
                commandModule.ParseCommand(command, false);
            if (invChanged)
            {
                invChanged = false;
                WumpusWindow.SetTextSafe(GetInventoryString(), inventoryWindow);
            }
        }

        public void GiveItem(ITEM_ID item, int count = 1)
        {
            if (inventory.ContainsKey(item))
                inventory[item] += count;
            else inventory.Add(item, count);
            invChanged = true;
        }

        public void RemoveItem(ITEM_ID item, int count = 1)
        {
            if (HasItemInInventory(item))
            {
                invChanged = true;
                inventory[item] -= inventory[item] > count ? count : inventory[item];
            }
        }

        public bool HasItemInInventory(ITEM_ID item)
        {
            return inventory.ContainsKey(item) && inventory[item] > 0;
        }

        void PeekCommand(List<string> arguments)
        {
            StringBuilder msg = new StringBuilder();
            var connectedRooms = CurrentRoom.GetConnectedRooms();

            msg.Append("Items in room:");
            foreach (var kvp in CurrentRoom.GetItemCounts())
            {
                msg.Append($"\r\n{kvp.Key} : {kvp.Value}");
            }

            msg.Append("\r\nConnected Rooms:");
            foreach (var room in connectedRooms)
            {
                msg.Append($"\r\n{room.index}");
            }

            WumpusGameManager.WriteLine(msg.ToString());
        }
        void MoveCommand(List<string> arguments) 
        {
            if (arguments.Count == 0)
            {
                WumpusGameManager.WriteLine($"Invalid Command Input: No room specified.");
                return;
            }

            if(int.TryParse(arguments[0], out int targetRoom))
            {
                if (CurrentRoom.IsConnectedTo(targetRoom))
                    SetRoom(CurrentRoom.GetConnectedRoom(targetRoom));
                else WumpusGameManager.WriteLine("Invalid Command Input: Target room is not connected.");
            }
            else WumpusGameManager.WriteLine("Invalid Command Input: Did not enter a number.");
        }
        void UseCommand(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                WumpusGameManager.WriteLine($"Invalid Command Input: No item specified.");
                return;
            }

            if (ItemManager.StringToItem(arguments[0], out ITEM_ID item))
            {
                if (HasItemInInventory(item))
                {
                    WumpusGameManager.WriteLine($"Used {item}!!");
                    switch(item)
                    {
                        case ITEM_ID.ARROW:
                            WumpusGameManager.WriteLine($"Invalid Command Input: Arrows cannot be used directly.");
                            break;
                        case ITEM_ID.BOW:
                            if(arguments.Count > 1 && int.TryParse(arguments[1], out int roomIndex))
                            {
                                ShootBow(roomIndex);
                            }
                            break;
                        case ITEM_ID.TORCH:
                            break;
                    }
                }
                else WumpusGameManager.WriteLine($"Invalid Command Input: \"{arguments[0]}\" is not in your inventory.");
            }
            else WumpusGameManager.WriteLine($"Invalid Command Input: \"{arguments[0]}\" is not a valid item.");
        }
        void PickCommand(List<string> arguments) 
        {
            if(arguments.Count == 0)
            {
                WumpusGameManager.WriteLine("Invalid Command Input: No item specified");
                return;
            }

            if (ItemManager.StringToItem(arguments[0], out ITEM_ID item))
            {
                if (CurrentRoom.ContainsItem(item))
                {
                    int amount = CurrentRoom.GetItemCount(item);
                    if (arguments.Count > 1 && int.TryParse(arguments[1], out int inputAmount) && inputAmount <= amount)
                        amount = inputAmount;
                    CurrentRoom.RemoveItem(item, amount);
                    GiveItem(item, amount);
                }
                else WumpusGameManager.WriteLine($"Invalid Command Input: {item} is not in the room.");
            }
            else WumpusGameManager.WriteLine($"Invalid Command Input: {arguments[0]} is not an item.");
        }
        void DropCommand(List<string> arguments) 
        {
            if (arguments.Count == 0)
            {
                WumpusGameManager.WriteLine("Invalid Command Input: No item specified");
                return;
            }

            if (ItemManager.StringToItem(arguments[0], out ITEM_ID item))
            {
                if (HasItemInInventory(item))
                {
                    int amount = inventory[item];
                    if (arguments.Count > 1 && int.TryParse(arguments[1], out int inputAmount) && inputAmount <= amount)
                        amount = inputAmount;
                    CurrentRoom.AddItem(item, amount);
                    RemoveItem(item, amount);
                }
                else WumpusGameManager.WriteLine($"Invalid Command Input: You do not have {item} in your inventory.");
            }
            else WumpusGameManager.WriteLine($"Invalid Command Input: {arguments[0]} is not an item.");
        }

        string GetInventoryString()
        {
            bool hasItems = false;
            StringBuilder msg = new StringBuilder();
            foreach (var kvp in inventory)
            {
                if (kvp.Value > 0)
                {
                    hasItems = true;
                    msg.Append($"\r\n{kvp.Key} * {kvp.Value}");
                }
            }
            if (!hasItems)
                msg.Append("\r\n*Empty*");
            return msg.ToString();
        }

        void ShootBow(int roomIndex)
        {
            BoardRoom nextRoom = CurrentRoom.GetConnectedRoom(roomIndex);
            if(nextRoom==null)
            {
                WumpusGameManager.WriteLine($"Invalid Command Input: Room {roomIndex} is not connected to room {CurrentRoom.index}");
                return;
            }
            if(!HasItemInInventory(ITEM_ID.ARROW))
            {
                WumpusGameManager.WriteLine($"You have no Arrows!");
                return;
            }

            RemoveItem(ITEM_ID.ARROW);
            if(nextRoom.ContainsEntityType<Wumpus>())
            {
                foreach (var wumpus in nextRoom.GetEntitiesInRoom<Wumpus>())
                    wumpus.TakeDamage(1);
            }
        }
    }
}
