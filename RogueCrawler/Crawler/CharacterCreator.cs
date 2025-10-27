using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    class CharacterCreator
    {
        public static Dictionary<AttributeType, string> attributeDescriptions = new Dictionary<AttributeType, string>()
        {
            [AttributeType.STR] = "+5 Carry weight. Get bonus damage for Axes and Blunt weapons.",
            [AttributeType.DEX] = "+0.2 Combat Speed. Bonus damage for Blades, Spears, Axes, and Ranged.",
            [AttributeType.CON] = $"+{DungeonCrawlerSettings.HitPointsPerConstitution} HP. Secondary stat for Blunt weapons.",
        };

        static string AttributeTypePrompt = $"Pick an attribute:\n\t{EnumExt<AttributeType>.Values.ToString(" | ")}\n";
        static EnumCommandModule<AttributeType> AttributeTypeCommands { get => EnumExt<AttributeType>.GetCommandModule(false); }

        public static PlayerCharacter CharacterCreationPrompt()
        {
            PlayerCharacter player = new PlayerCharacter()
            {
                ObjectName = "Default",
            };
            
            player.ObjectName = CommandManager.UserInputPrompt("Enter name", false);

            // Generate the starting weapon
            var wParams = ItemWeaponGenerationPresets.StartWeaponItem;
            string WeaponTypePrompt = $"Pick your preferred weapon:\n\t{WeaponTypeManager.WeaponTypes.Keys.ToString(" | ")}\n";
            wParams.PossibleWeaponTypes = new List<string>() { GetNextWeaponCommand(WeaponTypePrompt, $"[Invalid] Preferred Weapon", false) };
            player.PrimaryWeapon = DungeonGenerator.WeaponGenerator.Generate(wParams);

            player.Proficiencies.AddSkillLevel(player.PrimaryWeapon.WeaponType, 30);
            player.Proficiencies.AddSkillLevel(player.PrimaryWeapon.ObjectName, 30);

            // Set the starting attributes to the start weapon requirements
            player.AddAttributePoints(player.PrimaryWeapon.AttributeRequirements);

            // Generate player armor
            foreach (var slot in EnumExt<ArmorSlotType>.Values)
                player.Armor.EquipItem(DungeonGenerator.ArmorGenerator.GenerateUnarmoredSlot(slot));

            // Always add 1 to CON.
            player.AddAttributePoints(AttributeType.CON, 1);
            // Set the player to the smallest possible level, or the starting level. Whichever is greater.
            player.Level = Math.Max(player.MaxAttributes.CreatureLevel, DungeonCrawlerSettings.StartingPlayerLevel);

            // Allow player to apply any extra points
            int missingPoints = DungeonCrawlerSettings.AttributePointsPerCreatureLevel * player.Level - player.MaxAttributes.TotalScore;
            if (missingPoints > 0)
            {
                AttributePrompt(player, 0, missingPoints, 0);
            }

            // Set player to max health (otherwise we create a dead char)
            player.Health.SetPercent(1.0f);
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

            Console.WriteLine(player.MaxAttributes.DebugString("Your Current Attributes:", tabCount));
            while (attrPoints > 0)
            {
                AttributeType attr = GetNextAttributeCommand("Add point to: ", "[INVALID] Add point to: ", false);
                player.AddAttributePoints(attr, 1);
                --attrPoints;
                Console.WriteLine($"{attr} {player.MaxAttributes[attr] - 1} -> {player.MaxAttributes[attr]}");
            }

            Console.WriteLine(player.MaxAttributes.InspectString("All points distributed.\nNew attributes:", tabCount));
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
