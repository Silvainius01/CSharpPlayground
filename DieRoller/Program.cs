using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

namespace DieRoller
{
    class Program
    {
        static CancellationTokenSource ctFishing = new CancellationTokenSource();
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

            Task.Run(() => FishingTimer(ctFishing.Token));
            
            while (true)
            {
                //DiceRoller.DiceRollPrompt<DiceRoll>(DiceRoll.TryParse);

            }
        }

        static async Task FishingTimer(CancellationToken ct)
        {
            PeriodicTimer t = new PeriodicTimer(TimeSpan.FromSeconds(3.5));

            while (!ct.IsCancellationRequested)
            {
                Console.WriteLine("FISH!!!!");
                await t.WaitForNextTickAsync(ct);
            }
        }
    }
}
