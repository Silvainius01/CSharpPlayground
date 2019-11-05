using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace CSharpPlayground.Wumpus
{
    class Player : BoardEntity
    {
        EntityCommandModule commandModule;
        Dictionary<ITEM_ID, int> inventory = new Dictionary<ITEM_ID, int>();
        Task nextCommandTask =null;

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
            if(nextCommandTask == null || nextCommandTask.IsCompleted)
                nextCommandTask = commandModule.GetNextCommandAsync("Next Command", true, false);            
        }

        public void GiveItem(ITEM_ID item, int count = 1)
        {
            if (HasItemInInventory(item))
                inventory[item] += count;
            else inventory.Add(item, count);
        }

        public void RemoveItem(ITEM_ID item, int count = 1)
        {
            if (HasItemInInventory(item))
                inventory[item] -= inventory[item] > count ? count : inventory[item];
        }

        public bool HasItemInInventory(ITEM_ID item)
        {
            return inventory.ContainsKey(item) || inventory[item] > 0;
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
                    msg.Append($"\n\t{kvp.Key} * {kvp.Value}");
                }
            }
            if (!hasItems)
                msg.Append("\n\t*Empty*");

            Console.WriteLine(msg.ToString());
        }
    }
}
