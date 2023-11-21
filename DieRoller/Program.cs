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
            Console.WriteLine("Enter dice in the following format: XdY ZdW");
            Console.WriteLine("Example: 4d4 2d6");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("s  -> Roll Seperate");
            Console.WriteLine("i  -> Display Individual Rolls");
            Console.WriteLine("st -> Displate Seperate Stats");
            Console.WriteLine("t  -> Display Total Stats");
            Console.WriteLine("f  -> Display Full Stats");
            Console.WriteLine("-l [numDice]  -> Take only the lowest X dice");
            Console.WriteLine("-h [numDice]  -> Take only the highest X dice");
            Console.WriteLine("-r [value] [retries]  -> Reroll any dice that roll [value], up to the retry limit. Omitting the retry limit or entering 0 or less will retry indefinitely.");

            while (true)
            {
                DiceRoller.DiceRollPrompt<DiceRoll>(DiceRoll.TryParse);
            }

            //long kb = 1024;
            //long twoGb = (kb * kb * kb * 2) / 4; // Each ram is 4 bytes, so divide by 4 so we get 2gb of mem
            //StringBuilder ramString = new StringBuilder("🐏", 536870912);
            //StreamWriter writer = new StreamWriter("D:/Documents (Real)/2gb_of_ram.txt");

            //while(ramString.Length < ramString.Capacity)
            //{
            //    ramString.Append(ramString.ToString());
            //    Console.WriteLine($"Rams: {ramString.Length}");
            //}

            //Console.WriteLine("Write 1");
            //writer.Write(ramString.ToString());
            //Console.WriteLine("Write 2");
            //writer.Write(ramString.ToString());
        }
    }
}
