using System;
using System.Collections.Generic;
using System.ComponentModel;
using DieRoller;
using GameEngine;
using System.Linq;

namespace DnD_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter dice in the following format: XdY ZdW \nExample: 4d4 2d6");

            while (true)
            {
                //ModTest();
                //RandomTest();
                WeaponTest();
                //DungeonTest();
                //DiceRoller.DiceRollPrompt<StatRoll>(StatRoll.TryParse);
            }
        }

        int RollAttribute()
        {
            int lowest = 0;
            int total = 0;
            for (int i = 0; i < 4; ++i)
            {
                int r = DiceRoller.RollDice(1, 6);
                if (r < lowest)
                    lowest = r;
                total += r;
            }
            return total - lowest;
        }

        public List<int> RollPlayerAttributes()
        {
            List<int> attributes = new List<int>();
            for (int i = 0; i < 6; ++i)
                attributes.Add(RollAttribute());
            return attributes;
        }

        static int total = 0;
        static Dictionary<int, int> rCount = new Dictionary<int, int>(10)
            {
                [0] = 0,
                [1] = 0,
                [2] = 0,
                [3] = 0,
                [4] = 0,
                [5] = 0,
                [6] = 0,
                [7] = 0,
                [8] = 0,
                [9] = 0
            };
        static Dictionary<int, int> rCount2 = new Dictionary<int, int>(10)
            {
                [0] = 0,
                [1] = 0,
                [2] = 0,
                [3] = 0,
                [4] = 0,
                [5] = 0,
                [6] = 0,
                [7] = 0,
                [8] = 0,
                [9] = 0
            };
        public static void RandomTest()
        {
            int numRolls = 10;
            total += numRolls;
            for (int i = 0; i < numRolls; ++i)
            {
                float rFloat = Mathc.Random.NextFloat(new Vector2(-10.0f, 10.0f));
                //Console.WriteLine(Mathc.Random.NormalDouble);
                ++rCount[(int)Mathc.Mod(rFloat, 10.0f)];
                //++rCount2[(int)rFloat % 2];
                Console.WriteLine(rFloat);
            }
            for (int i = 0; i < 10; ++i)
            {
                //Console.WriteLine($"{i}: {(rCount[i] / (float)total) * 100} vs {(rCount2[i] / (float)total) * 100}");
                //Console.WriteLine($"{i}: {(rCount[i] / (float)total) * 100}");
            }
            Console.ReadLine();
        }

        public static void ModTest()
        {

            string input = Console.ReadLine();
            string[] modOptionsRaw = input.Split(' ');
            float toMod, modBase;

            if(modOptionsRaw.Length > 1 && float.TryParse(modOptionsRaw[1], out modBase))
            {
                while(modOptionsRaw[0] != "e")
                {
                    if (float.TryParse(modOptionsRaw[0], out toMod))
                        Console.WriteLine($"{toMod} % {modBase} = {Mathc.Mod(toMod, modBase)}");
                    else Console.WriteLine($"{modOptionsRaw[0]} is a valid float.");

                    modOptionsRaw = Console.ReadLine().Split(' ');
                }
            }
        }

        public static void DungeonTest()
        {
            //Dungeon d = new Dungeon(26);

            DungeonGenerationParameters dParams = new DungeonGenerationParameters()
            {
                RoomRange = new Vector2Int(10*9, 10*10),
                ConnectionRange = new Vector2Int(1, 3),
                RoomHeightRange = new Vector2Int(1, 1),
                RoomWidthRange = new Vector2Int(1, 1)
            };
            Dungeon d2 = DungeonGenerator.GenerateDungeon(dParams);

            //d.dimensions = new GameEngine.Vector2Int(5, 5);
            Console.WriteLine(d2.DebugString());
            string s = Console.ReadLine();
        }

        public static void WeaponTest()
        {
            ItemWeaponGenerationProperties properties = new ItemWeaponGenerationProperties()
            {
                QualityRange = new Vector2Int(0, 50),
                WeightRange = new Vector2Int(25, 100),
                LargeWeaponProbability = 50
            };

            var weapon = ItemWeaponGenerator.GenerateWeapon(properties);
            Console.WriteLine(weapon.DebugString());
            Console.ReadLine();
        }

    }
}
