using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;

namespace RogueCrawler
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
            builder.NewlineAppend(tabCount, $"Damage: {GetCreatureDamage()}");
            builder.NewlineAppend(PrimaryWeapon.InspectString($"Weapon Stats:", tabCount));
            builder.NewlineAppend(MaxAttributes.InspectString("Attributes:", tabCount));

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
            builder.NewlineAppend(tabCount, $"Damage: {GetCreatureDamage()}");

            return builder.ToString();
        }

        public override SerializedCreature GetSerializable()
        {
            return new SerializedCharacter(this);
        }
    }

    class SerializedCharacter : SerializedCreature
    {
        public int Experience { get; set; }

        public SerializedCharacter() : base() { }
        public SerializedCharacter(PlayerCharacter pc) : base(pc)
        {
            Experience = pc.Experience;
        }

        public override Creature GetDeserialized()
        {
            return base.GetDeserialized();
        }
    }
}
