using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameEngine;

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
            if (HasItemInInventory(item))
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

        void PeekCommand(List<string> arguments) { }
        void MoveCommand(List<string> arguments) { }
        void UseCommand(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                WumpusGameManager.WriteLine($"Invalid Command Format: No item specified.");
                return;
            }

            if (ItemManager.StringToItem(arguments[0], out ITEM_ID item))
            {
                if(HasItemInInventory(item))
                    WumpusGameManager.WriteLine($"Used {item}!!");
                else WumpusGameManager.WriteLine($"Invalid Command Input: \"{arguments[0]}\" is not in your inventory.");
            }
            else WumpusGameManager.WriteLine($"Invalid Command Input: \"{arguments[0]}\" is not a valid item.");
        }
        void PickCommand(List<string> arguments) { }
        void DropCommand(List<string> arguments) { }

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
    }
}
