using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;

namespace DnD_Generator
{
    class CharacterCreator
    {
        public static Dictionary<AttributeType, string> attributeDescriptions = new Dictionary<AttributeType, string>()
        {
            [AttributeType.STR] = "+5 Carry weight. Get bonus damage for Axes and Blunt weapons.",
            [AttributeType.DEX] = "Get bonus damage for Blades and Ranged weapons.",
            [AttributeType.CON] = $"+{DungeonCrawlerSettings.HitPointsPerConstitution} HP. Secondary stat for Blunt weapons.",
        };

        static string AttributeTypePrompt = $"Pick an attribute:\n\t{EnumExt<AttributeType>.Values.ToString(" | ")}\n";
        static EnumCommandModule<AttributeType> AttributeTypeCommands { get => EnumExt<AttributeType>.GetCommandModule(false); }

        public static PlayerCharacter CharacterCreationPrompt()
        {
            PlayerCharacter player = new PlayerCharacter()
            {
                WeaponName = "Default",
                HitPoints = DungeonCrawlerSettings.MinCreatureHitPoints,
                ArmorClass = 0,
            };
            
            player.WeaponName = CommandManager.UserInputPrompt("Enter name", false);

            // Generate the starting weapon
            var wParams = ItemWeaponGenerationPresets.StartWeaponItem;
            string WeaponTypePrompt = $"Pick your preferred weapon:\n\t{WeaponTypeManager.WeaponTypes.Keys.ToString(" | ")}\n";
            wParams.PossibleWeaponTypes = new List<string>() { GetNextWeaponCommand(WeaponTypePrompt, $"[Invalid] Preferred Weapon", false) };
            player.PrimaryWeapon = DungeonGenerator.GenerateWeapon(wParams);

            // Set the starting attributes to the start weapon requirements
            player.Attributes = new CrawlerAttributeSet(player.PrimaryWeapon.AttributeRequirements);
            // Always add 1 to CON.
            player.Attributes[AttributeType.CON] += 1;
            // Set the player to the minimum possible creature level
            player.Level = player.Attributes.CreatureLevel;

            // Allow player to apply any extra points
            int missingPoints = player.Attributes.GetMissingPoints(DungeonCrawlerSettings.AttributePointsPerCreatureLevel);
            if (missingPoints > 0)
            {
                AttributePrompt(player, 0, missingPoints, 0);
            }

            // Set player to max health (otherwise we create a dead char)
            player.HitPoints = player.MaxHitPoints;
            return player;
        }

        public static void AttributePrompt(PlayerCharacter player, int levelsGained, int attrPoints, int tabCount)
        {
            SmartStringBuilder staticBuilder = new SmartStringBuilder(DungeonCrawlerSettings.TabString);

            staticBuilder.Clear();
            if(levelsGained > 0)
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

        public static string GetNextWeaponCommand(string firstPrompt, string failPrompt, bool newline)
        {
            var WeaponTypeCommands = WeaponTypeManager.WeaponTypeCommandModule;
            bool success = WeaponTypeCommands.NextCommand(firstPrompt, newline, out WeaponTypeData type);
            while (!success)
                success = WeaponTypeCommands.NextCommand(failPrompt, newline, out type);
            return type.WeaponType;
        }
        public static AttributeType GetNextAttributeCommand(string firstPrompt, string failPrompt, bool newline)
        {
            bool success = AttributeTypeCommands.NextCommand(firstPrompt, newline, out AttributeType type);
            while (!success)
                success = AttributeTypeCommands.NextCommand(failPrompt, newline, out type);
            return type;
        }
    }
}
