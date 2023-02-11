using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;

namespace RogueCrawler
{
    class DungeonChest<TItem> : IInspectable, IDungeonObject where TItem : IItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DungeonChestType Type { get; set; }

        public bool Inspected = false;
        public int ItemCount
        {
            get
            {
                if (updateCount)
                    itemCount = Items.Aggregate(0, (count, kvp) => kvp.Value.Count + count);
                return itemCount;
            }
        }
        public Dictionary<int, (TItem Item, int Count)> Items = new Dictionary<int, (TItem Item, int Count)>();

        int itemCount;
        bool updateCount = true;

        public void AddItem(TItem item, int amount = 1)
        {
            if (Items.ContainsKey(item.ID))
                AddItemCount(item, amount);
            else Items.Add(item.ID, (item, amount));
            updateCount = true;
        }
        public bool RemoveItem(int itemId, out TItem item)
        {
            if (Items.ContainsKey(itemId))
            {
                var kvp = Items[itemId];
                item = kvp.Item;

                if(kvp.Count > 1)
                {
                    AddItemCount(item, -1);
                    return true;
                }
                if (kvp.Count == 1)
                    return Items.Remove(itemId);

                // Return false if the count is >= 0
                return false;
            }

            item = default(TItem);
            return false;
        }
        public bool RemoveAllItems(int itemId, out (TItem item, int count) item)
        {
            if (Items.ContainsKey(itemId))
            {
                item = Items[itemId];
                Items.Remove(itemId);
                return true;
            }
            item = (default(TItem), 0);
            return false;
        }

        public bool ContainsItem(int itemId) => Items.ContainsKey(itemId) && Items[itemId].Count > 0;
        public List<IItem> RemoveAllItems()
        {
            List<IItem> allItems = new List<IItem>(ItemCount);
            foreach (var kvp in Items)
                for (int i = 0; i < kvp.Value.Count; ++i)
                    allItems.Add(kvp.Value.Item);
            Items.Clear();
            return allItems;
        }

        public TItem GetItem(int itemId) => Items[itemId].Item;

        void SetItemCount(TItem item, int count)
        {
            SetItemCount(item.ID, count);
        }
        void SetItemCount(int itemId, int count)
        {
            var kvp = Items[itemId];
            Items[itemId] = (kvp.Item, count);
        }
        void AddItemCount(TItem item, int amount)
        {
            SetItemCount(item.ID, Items[item.ID].Count + amount);
        }
        public int GetItemCount(TItem item) => ContainsItem(item.ID) ? Items[item.ID].Count : 0;

        public void MarkInspected()
        {
            Inspected = true;
        }

        public override string ToString()
        {
            return $"[{ID}] {Name}";
        }
        public string BriefString()
        {
            if (Inspected && ItemCount == 0)
                return $"(EMPTY) {ToString()}";
            return $"{ToString()}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"Items in {Name}: ";

            builder.Append(tabCount, prefix);

            tabCount++;
            foreach (var kvp in Items)
            {
                var item = kvp.Value.Item;
                int count = kvp.Value.Count;
                if(count > 1)
                    builder.NewlineAppend(tabCount, $"{count} x {item.BriefString()}");
                else builder.NewlineAppend(tabCount, $"{item.BriefString()}");
            }
            --tabCount;

            return builder.ToString();
        }
        public string DebugString(string prefix, int tabCount)
        {
            return InspectString(prefix, tabCount);
        }
    }
}
