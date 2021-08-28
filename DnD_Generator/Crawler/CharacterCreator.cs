using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    class CharacterCreator
    {
        public static Dictionary<AttributeType, string> attributeDescriptions = new Dictionary<AttributeType, string>()
        {
            [AttributeType.STR] = "+5 carry weight. Get bonus damage for Axes and Blunt weapons.",
            [AttributeType.DEX] = "Get bonus damage for Blades and Ranged weapons, and make it easier to hit things.",
            [AttributeType.CON] = $"+{DungeonCrawlerSettings.HitPointsPerConstitution} HP. Secondary stat for Blunt weapons.",
        };

        static string WeaponTypePrompt = $"Pick your preferred weapon:\n\t{EnumExt<WeaponType>.Values.ToString(" | ")}\n";
        static EnumCommandModule<WeaponType> WeaponTypeCommands { get => EnumExt<WeaponType>.GetCommandModule(false); }

        static string AttributeTypePrompt = $"Pick an attribute:\n\t{EnumExt<AttributeType>.Values.ToString(" | ")}\n";
        static EnumCommandModule<AttributeType> AttributeTypeCommands { get => EnumExt<AttributeType>.GetCommandModule(false); }

        public static PlayerCharacter CharacterCreationPrompt()
        {
            PlayerCharacter player = new PlayerCharacter()
            {
                Level = 2,
                Name = "Default",
                HitPoints = DungeonCrawlerSettings.MinCreatureHitPoints,
                ArmorClass = 0,
            };
            
            player.Name = CommandManager.UserInputPrompt("Enter name", false);

            var wParams = ItemWeaponGenerationPresets.StartWeaponItem;
            wParams.PossibleWeaponTypes = new List<WeaponType>() { GetNextWeaponCommand(WeaponTypePrompt, $"[Invalid] Preferred Weapon", false) };
            player.PrimaryWeapon = DungeonGenerator.GenerateWeapon(wParams);

            player.Attributes = new CreatureAttributes(player.PrimaryWeapon.AttributeRequirements);
            int attrPoints = player.Attributes.Level * (DungeonCrawlerSettings.AttributePointsPerLevel) - player.Attributes.TotalScore;
            if (attrPoints <= 0)
            {
                player.Level = player.Attributes.Level + 1;
                attrPoints = 2;
            }
            else player.Level = player.Attributes.Level;

            player.Attributes[AttributeType.CON] += attrPoints;
            player.HitPoints = player.MaxHitPoints;

            return player;
        }

        public static void AttributePrompt(PlayerCharacter player, int levelsGained, int attrPoints, int tabCount)
        {
            SmartStringBuilder staticBuilder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

            staticBuilder.Clear();
            staticBuilder.Append(tabCount, $"\nYou've gained {levelsGained} levels!");
            staticBuilder.NewlineAppend(tabCount, $"You have {attrPoints} attribute points to allocate as you choose.");

            ++tabCount;
            foreach (var value in EnumExt<AttributeType>.Values)
                if (attributeDescriptions.ContainsKey(value))
                    staticBuilder.NewlineAppend(tabCount, $"{value}: {attributeDescriptions[value]}");
            else staticBuilder.NewlineAppend(tabCount, $"{value}: Unused.");
            --tabCount;

            Console.WriteLine(staticBuilder.ToString());
            staticBuilder.Clear();

            Console.WriteLine(player.Attributes.DebugString("Your Current Attributes:", tabCount));
            while (attrPoints > 0)
            {
                AttributeType attr = GetNextAttributeCommand("Add point to: ", "[INVALID] Add point to: ", false);
                ++player.Attributes[attr];
                --attrPoints;
                Console.WriteLine($"{attr} {player.Attributes[attr] - 1} -> {player.Attributes[attr]}");
            }

            Console.WriteLine(player.Attributes.InspectString("All points distributed.\nNew attributes:", tabCount));
        }

        public static WeaponType GetNextWeaponCommand(string firstPrompt, string failPrompt, bool newline)
        {
            bool success = WeaponTypeCommands.TryGetValueFromCommand(firstPrompt, newline, out WeaponType type);
            while (!success)
                success = WeaponTypeCommands.TryGetValueFromCommand(failPrompt, newline, out type);
            return type;
        }
        public static AttributeType GetNextAttributeCommand(string firstPrompt, string failPrompt, bool newline)
        {
            bool success = AttributeTypeCommands.TryGetValueFromCommand(firstPrompt, newline, out AttributeType type);
            while (!success)
                success = AttributeTypeCommands.TryGetValueFromCommand(failPrompt, newline, out type);
            return type;
        }
    }
}
