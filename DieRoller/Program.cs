using System;
using System.IO;
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
            //Console.WriteLine("Enter dice in the following format: XdY ZdW \nExample: 4d4 2d6");

            //while (true)
            //{
            //    DiceRoller.DiceRollPrompt<DiceRoll>(DiceRoll.TryParse);
            //}

            long kb = 1024;
            long twoGb = (kb * kb * kb * 2) / 4; // Each ram is 4 bytes, so divide by 4 so we get 2gb of mem
            StringBuilder ramString = new StringBuilder("🐏", 536870912);
            StreamWriter writer = new StreamWriter("D:/Documents (Real)/2gb_of_ram.txt");
            
            while(ramString.Length < ramString.Capacity)
            {
                ramString.Append(ramString.ToString());
                Console.WriteLine($"Rams: {ramString.Length}");
            }

            Console.WriteLine("Write 1");
            writer.Write(ramString.ToString());
            Console.WriteLine("Write 2");
            writer.Write(ramString.ToString());
        }
    }
}
