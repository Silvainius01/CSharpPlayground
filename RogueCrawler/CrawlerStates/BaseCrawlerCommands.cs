using System;
using System.Collections.Generic;
using System.Text;

namespace RogueCrawler
{
    abstract class BaseCrawlerStateManager
    {
        protected DungeonCrawlerManager crawlerManager;

        protected Dungeon dungeon
        {
            get => crawlerManager.dungeon;
            set { crawlerManager.dungeon = value; }
        }
        protected PlayerCharacter player
        {
            get => crawlerManager.player;
            set { crawlerManager.player = value; }
        }

        public BaseCrawlerStateManager(DungeonCrawlerManager manager)
        {
            crawlerManager = manager;
        }

        public abstract void StartCrawlerState();
        public abstract CrawlerState UpdateCrawlerState();
        public abstract void EndCrawlerState();

        protected bool BaseCreatureCommand(List<string> args, out Creature creature, out string errorMsg)
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
                if (creature is not null)
                {
                    errorMsg = string.Empty;
                    return true;
                }
            }

            creature = null;
            errorMsg += (". Not a valid creature ID");
            return false;
        }

        protected bool BaseChestCommand(List<string> args, out DungeonChest<IItem> chest, out string errorMsg)
        {
            if (BaseIntCommand(args, out int chestId, out errorMsg))
            {
                if (dungeon.chestManager.GetObjectCount(player.CurrentRoom) == 0)
                {
                    errorMsg = ("There are no chests in this room.");
                    chest = null;
                    return false;
                }

                chest = dungeon.chestManager.GetObjectInRoom(player.CurrentRoom, chestId);
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

        protected bool BaseItemCommand(List<string> args, out IItem item, out DungeonChest<IItem> containingChest, out string errorMsg)
        {
            item = default(IItem);
            containingChest = null;

            if (BaseIntCommand(args, out int itemId, out errorMsg))
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
        protected bool BaseItemCommand(List<string> args, out IItem item, out string errorMsg)
            => BaseItemCommand(args, out item, out DungeonChest<IItem> swallowedRef, out errorMsg);

        protected bool BaseRoomCommand(List<string> args, out DungeonRoom room, out string errorMsg)
        {
            room = null;
            if (BaseIntCommand(args, out int index, out errorMsg))
            {
                room = dungeon.roomManager.GetRoomByIndex(index);
                if (room == null)
                {
                    errorMsg = "Not a valid room index.";
                    return false;
                }
                else if(!player.CurrentRoom.ConnectedTo(room))
                {
                    errorMsg = "No connection to this room!";
                    return false;
                }

                return true;
            }
            errorMsg = ("Not a valid room index.");
            return false;
        }

        protected bool BaseInventoryItemCommand(List<string> args, out IItem item, out string errorMsg)
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

        protected bool BaseNoCreatureCommand(out string errorMsg)
        {
            errorMsg = "Cannot use this commands while creatures are in the room!";
            return !dungeon.creatureManager.RoomContainsObjects(player.CurrentRoom);
        }

        protected bool BaseIntCommand(List<string> args, out int result, out string errorMsg)
        {
            result = 0;
            errorMsg = "Please enter a valid integer";
            if (args.Count > 0 && int.TryParse(args[0], out result))
            {
                return true;
            }
            return false;
        }
    }
}
