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
        Task nextCommandTask =null;
        TextBox inventoryWindow;

        public Player(TextBox invWindow)
        {
            inventoryWindow = invWindow;
        }

        public override void Awake()
        {
            commandModule = entity.AddComponent<EntityCommandModule>();
            commandModule.RegisterCommand(new ConsoleCommand { Name = "Use", ExecuteDelegate = UseCommand });
            commandModule.RegisterCommand(new ConsoleCommand { Name = "Peek", ExecuteDelegate = PeekCommand });
            commandModule.RegisterCommand(new ConsoleCommand { Name = "Move", ExecuteDelegate = MoveCommand });
            commandModule.RegisterCommand(new ConsoleCommand { Name = "Pickup", ExecuteDelegate = PickCommand });
            commandModule.RegisterCommand(new ConsoleCommand { Name = "Drop", ExecuteDelegate = DropCommand });
            commandModule.RegisterCommand(new ConsoleCommand { Name = "Inventory", ExecuteDelegate = InventoryCommand });
        }

        public override void Update()
        {
            string command = WumpusGameManager.GetNextCommand();
            if (command != null)
                commandModule.ParseCommand(command, false);          
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

        void PeekCommand(string input) { }
        void MoveCommand(string input) { }
        void UseCommand(string input) { }
        void PickCommand(string input) { }
        void DropCommand(string input) { }
        void InventoryCommand(string input)
        {
            bool hasItems = false;
            StringBuilder msg = new StringBuilder($"Inventory: ");
            foreach(var kvp in inventory)
            {
                if (kvp.Value > 0)
                {
                    hasItems = true;
                    msg.Append($"\r\n\t{kvp.Key} * {kvp.Value}");
                }
            }
            if (!hasItems)
                msg.Append("\r\n\t*Empty*");

            WumpusGameManager.WriteLine(msg.ToString());
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
    }
}
