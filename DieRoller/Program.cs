using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DieRoller
{
    class Program
    {
        static void Main(string[] args)
        {
            //DiceRoller.GetRollStats(4, 2);

            Console.WriteLine("Enter dice in the following format: XdY ZdW \nExample: 4d4 2d6");


            while (true)
            {
                Console.WriteLine("\n\nEnter Dice to Roll:");
                string input = Console.ReadLine();

                string[] diceRollsRaw = input.Split(' ', 'd');

                List<(int NumDice, int NumSides)> diceRolls = new List<(int NumDice, int NumSides)>(diceRollsRaw.Length / 2);
                for (int i = 0; i < diceRollsRaw.Length - 1; i += 2)
                {
                    diceRolls.Add((int.Parse(diceRollsRaw[i]), int.Parse(diceRollsRaw[i + 1])));
                }

                int result = DiceRoller.RollDice(diceRolls);
                Console.WriteLine($"Result: {result}");
                DiceRoller.GetRollStats(diceRolls);
            }

            //var vaxDamage = new List<(int NumDice, int NumSides)>()
            //{
            //    // Whisper
            //    (1, 4),
            //    (1, 8),
            //    // Dragon Slaying Sword
            //    (1, 8),
            //    (3, 6),
            //    // Sneak Attack
            //    (7, 6)
            //};

            //var jmonBreath = (16, 6);

            //DiceRoller.GetRollStats(jmonBreath);

            ////for(int i = 0; i < 10; ++i)
            ////{
            ////    int dmg = DiceRoller.RollDice(vaxDamage);
            ////    Console.WriteLine(dmg);
            ////}

            //Console.ReadLine();
        }
    }
}
