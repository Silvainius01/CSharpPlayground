using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CommandEngine;
using Morrowind.Data;

namespace Morrowind
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CommandModule commands = new CommandModule("\nNext Command");

            commands.Add("char", GenerateRandomCharacter);

            SkillManager.LoadTypes();
            SpellEffectManager.LoadTypes();
            IngredientManager.LoadTypes();

            FactionManager.LoadTypes();
            CharacterRaceManager.LoadTypes();
            MerchantManager.LoadTypes();

            while (true)
            {
                commands.NextCommand(false);
            }
        }

        public static void GenerateRandomCharacter(List<string> args)
        {
            RandomizerOptions options = new RandomizerOptions()
            {
                DisabledSkills = new List<string>()
                {
                    SkillManager.SkillNameArmorer,
                    SkillManager.SkillNameHandToHand,
                }
            };
            var character = CharacterRandomizer.GenerateCharacter(options, true);
        }
    }
}
