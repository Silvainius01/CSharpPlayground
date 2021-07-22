using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator.Crawler
{
    class CharacterCreator
    {
        public const string attrStrDesc = "Determines your ability to weild heavier weapons, and usually determines damage bonus in melee";
        public const string attrDexDesc = "Determines your ability to successgully land attacks, and aim ranged weapons.";
        public const string attrConDesc = "Is used in calculating your HP, and determines your overall resiliance.";
        public const string attrIntDesc = "Unused thus far.";
        public const string attrWisDesc = "Unused thus far.";
        public const string attrChaDesc = "Unused thus far.";

        public static Dictionary<string, ConsoleCommand<(AttributeType, int)>> creationCommands = new Dictionary<string, ConsoleCommand<(AttributeType, int)>>()
        {
            ["attr"] = new ConsoleCommand<(AttributeType, int)>("attr", null),
        };
        public static Dictionary<string, ConsoleCommand<WeaponType>> weaponTypeCommands = new Dictionary<string, ConsoleCommand<WeaponType>>()
        {
            ["axe"] = new ConsoleCommand<WeaponType>("axe", (List<string> args) => WeaponType.Axe),
            ["blunt"] = new ConsoleCommand<WeaponType>("blunt", (List<string> args) => WeaponType.Blunt),
            ["blade"] = new ConsoleCommand<WeaponType>("blade", (List<string> args) => WeaponType.Blade),
            ["ranged"] = new ConsoleCommand<WeaponType>("ranged", (List<string> args) => WeaponType.Ranged),
        };

        public static PlayerCharacter CharacterCreationPrompt()
        {
            PlayerCharacter character = new PlayerCharacter()
            {
                Name = "Default",
                HitPoints = 15,
                ArmorClass = 0,
                Attributes = new CreatureAttributes(5)
            };


            character.Name = CommandManager.UserInputPrompt("Enter name", false);
            character.PrimaryWeapon = ItemWeaponGenerator.GenerateWeapon(
                new ItemWeaponGenerationProperties()
                {
                    WeightRange = new Vector2Int(25, 25),
                    QualityRange = new Vector2Int(1, 1),
                    LargeWeaponProbability = ItemWeaponGenerationPresets.NoLargeRate,
                    PossibleWeaponTypes = new List<WeaponType>() { GetPreferredWeaponType() }
                }
            );

            return character;
        }

        public static WeaponType GetPreferredWeaponType()
        {
            StringBuilder promptBuilder = new StringBuilder("Pick your preferred weapon:");

            foreach (var weaponType in Mathc.GetEnumValues<WeaponType>())
            {
                promptBuilder.Append($"\n\t- {weaponType}");
            }
            promptBuilder.Append('\n');

            bool success = CommandManager.GetNextCommand(promptBuilder.ToString(), false, weaponTypeCommands, out WeaponType type);
            while (!success)
                success = CommandManager.GetNextCommand("Invalid Weapon Type. Try Again: ", false, weaponTypeCommands, out type);
            return type;
        }

        public static (AttributeType attr, int amt) AttributeCommand(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Not enough arguments, expected at least 1.");
                Console.ForegroundColor = color;
                return (AttributeType.STR, 0);
            }

            for (int i = 0; i < arguments.Count; ++i)
                arguments[0] = arguments[0].ToLower();

            if (arguments.Count > 1 && int.TryParse(arguments[1], out int amt))
            {
                switch (arguments[0].ToLower())
                {
                    case "str": return (AttributeType.STR, amt);
                    case "dex": return (AttributeType.DEX, amt);
                    case "con": return (AttributeType.CON, amt);
                    case "int": return (AttributeType.INT, amt);
                    case "wis": return (AttributeType.WIS, amt);
                    case "cha": return (AttributeType.CHA, amt);
                }
            }
            else if(arguments[0] == "help")
            {
                Console.WriteLine("Usage: attr [str|dex|con|int|wis|cha] [amount]");
            }

            return (AttributeType.STR, 0);
        }
    }
}
