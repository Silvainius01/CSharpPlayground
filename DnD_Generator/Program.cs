using System;
using System.Collections.Generic;
using System.ComponentModel;
using DieRoller;

namespace DnD_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter dice in the following format: XdY ZdW \nExample: 4d4 2d6");

            while (true)
            {
                DiceRoller.DiceRollPrompt<StatRoll>(StatRoll.TryParse);
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
    }
}
