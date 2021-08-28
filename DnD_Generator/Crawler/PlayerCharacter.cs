using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameEngine;

namespace DnD_Generator
{
    class PlayerCharacter : Creature
    {
        public int Experience { get; set; } = 0;
        public int ExperienceNeeded { get => 100 + ((Level - 1) * 200); }
        public int FurthestRoomExplored { get; set; }
        public int CreaturesKilled { get; set; }
        public HashSet<int> BorderRooms = new HashSet<int>();
        public HashSet<int> CheckedRooms = new HashSet<int>();
        public HashSet<int> ExploredRooms = new HashSet<int>();

        public bool RoomIsExplorable(DungeonRoom room)
        {
            return ExploredRooms.Contains(room.Index) || RoomBordersExploredRoom(room);
        }
        public bool RoomBordersExploredRoom(DungeonRoom room)
        {
            bool connected = false;
            foreach (var id in room.connections.Keys)
                connected |= ExploredRooms.Contains(id);
            return connected;
        }

        public void ResetDungeonStats()
        {
            FurthestRoomExplored = 0;
            CreaturesKilled = 0;
            BorderRooms.Clear();
            CheckedRooms.Clear();
            ExploredRooms.Clear();
        }

        public override string InspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);;

            if (prefix == string.Empty)
                prefix = $"Stats for {Name}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"HP: {HitPoints}/{MaxHitPoints}");
            builder.NewlineAppend(tabCount, $"Level: {Level}");
            builder.NewlineAppend(tabCount, $"Damage: {Damage}");
            builder.NewlineAppend(PrimaryWeapon.InspectString($"Weapon Stats:", tabCount));
            builder.NewlineAppend(Attributes.InspectString("Attributes:", tabCount));

            return builder.ToString();
        }

        public string BriefInspectString(string prefix, int tabCount)
        {
            SmartStringBuilder builder = new SmartStringBuilder(DungeonCrawlerSettings.TabString); ;

            if (prefix == string.Empty)
                prefix = $"Stats for {Name}:";

            builder.Append(tabCount, prefix);
            tabCount++;
            builder.NewlineAppend(tabCount, $"HP: {HitPoints}/{MaxHitPoints}");
            builder.NewlineAppend(tabCount, $"Level: {Level}");
            builder.NewlineAppend(tabCount, $"Damage: {Damage}");

            return builder.ToString();
        }
    }
}
