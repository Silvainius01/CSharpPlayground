using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DieRoller
{
    public class DiceRoller
    {
        static Random rng = new Random();

        public static int RollDice(int numDice, int numSides)
        {
            int result = 0;
            for (int i = 0; i < numDice; ++i)
                result += rng.Next(numSides) + 1;
            return result;
        }
        public static int RollDice((int NumDice, int NumSides) diceRoll)
        {
            return RollDice(diceRoll.NumDice, diceRoll.NumSides);
        }
        public static int RollDice(IEnumerable<(int NumDice, int NumSides)> diceRolls)
        {
            int result = 0;
            foreach (var diceRoll in diceRolls)
                result += RollDice(diceRoll);
            return result;
        }

        public static void GetRollStats(int numDice, int numSides)
        {
            double avg = GetRollAverage(numDice, numSides);
            (int min, int max) range = GetRollRange(numDice, numSides);
            Console.WriteLine($"Avg: {avg}\nRange: {range.min}-{range.max}");
        }
        public static void GetRollStats((int NumDice, int NumSides) diceRoll)
        {
            GetRollStats(diceRoll.NumDice, diceRoll.NumSides);
        }
        public static void GetRollStats(IEnumerable<(int NumDice, int NumSides)> diceRolls)
        {
            double avg = GetRollAverage(diceRolls);
            (int min, int max) range = GetRollRange(diceRolls);
            Console.WriteLine($"Avg: {avg}\nRange: {range.min}-{range.max}");
        }

        public static void GetFullRollStats(int numDice, int numSides)
        {
            GetFullRollStatsInternal(GetDiceSides(numDice, numSides));
        }
        public static void GetFullRollStats((int NumDice, int NumSides) diceRoll)
        {
            GetFullRollStatsInternal(GetDiceSides(diceRoll));
        }
        public static void GetFullRollStats(IEnumerable<(int NumDice, int NumSides)> diceRolls)
        {
            GetFullRollStatsInternal(GetDiceSides(diceRolls));
        }
        static void GetFullRollStatsInternal(IEnumerable<int> resultEnum)
        {
            double average = 0;
            int totalRolls = 0;
            Dictionary<int, int> totals = new Dictionary<int, int>();

            foreach (var rollResult in resultEnum)
            {
                if (totals.ContainsKey(rollResult))
                {
                    ++totals[rollResult];
                }
                else totals.Add(rollResult, 1);

                ++totalRolls;
                average += rollResult;
            }
            average /= totalRolls;

            var sortedKeyList = totals.Keys.ToList();
            sortedKeyList.Sort();
            foreach (var key in sortedKeyList)
            {
                double chance = ((double)totals[key] / (double)totalRolls) * 100;
                Console.WriteLine($"{key}: {totals[key]} ({chance}%)");
            }
            Console.WriteLine($"Avg: {average}");
        }

        public static double GetRollAverage(int numDice, int numSides)
        {
            double avg = (numSides / 2) + 0.5;
            return avg * numDice;
        }
        public static double GetRollAverage((int NumDice, int NumSides) diceRoll) 
        {
            return GetRollAverage(diceRoll.NumDice, diceRoll.NumSides);
        }
        public static double GetRollAverage(IEnumerable<(int NumDice, int NumSides)> diceRolls)
        {
            double avg = 0;
            foreach (var diceRoll in diceRolls)
                avg += GetRollAverage(diceRoll);
            return avg;
        }

        public static (int min, int max) GetRollRange(int numDice, int numSides)
        {
            return (numDice, numDice * numSides);
        }
        public static (int min, int max) GetRollRange((int NumDice, int NumSides) diceRoll)
        {
            return GetRollRange(diceRoll.NumDice, diceRoll.NumSides);
        }
        public static (int min, int max) GetRollRange(IEnumerable<(int NumDice, int NumSides)> diceRolls)
        {
            (int min, int max) range = (0,0);
            foreach (var diceRoll in diceRolls)
            {
                var r = GetRollRange(diceRoll);
                range = (range.min + r.min, range.max + r.max);
            }
            return range;
        }

        static IEnumerable<int> GetDieSides(int numSides)
        {
            for (int i = 0; i < numSides; ++i)
                yield return i + 1;
        }
        static IEnumerable<int> GetDiceSides(int numDice, int numSides)
        {
            var sideEnum = GetDieSides(numSides);
            var nextDiceEnum = GetDiceSides(numDice - 1, numSides);

            foreach (var side in sideEnum)
            {
                int total = side;

                if (numDice > 1)
                {
                    foreach (var nextDiceTotal in nextDiceEnum)
                        yield return total + nextDiceTotal;
                }
                else yield return total;
            }
        }
        static IEnumerable<int> GetDiceSides((int NumDice, int NumSides) diceRoll)
        {
            return GetDiceSides(diceRoll.NumDice, diceRoll.NumSides);
        }
        static IEnumerable<int> GetDiceSides(IEnumerable<(int NumDice, int NumSides)> diceRolls)
        {
            return GetDiceSides(diceRolls, 0, diceRolls.Count());

            //int min = diceRolls.Count();
            //int max = 0;
            //foreach(var diceRoll in diceRolls)
            //    max += diceRoll.NumDice * diceRoll.NumSides;


        }
        static IEnumerable<int> GetDiceSides(IEnumerable<(int NumDice, int NumSides)> diceRolls, int currIndex, int count)
        {
            var diceRollEnum = GetDiceSides(diceRolls.ElementAt(currIndex));
            if (currIndex < count - 1)
            {
                var nextDiceEnum = GetDiceSides(diceRolls, currIndex + 1, count);

                foreach (var result in diceRollEnum)
                {
                    foreach (var nextResult in nextDiceEnum)
                        yield return result + nextResult;
                }
            }
            else
            {
                foreach (var result in diceRollEnum)
                    yield return result;
            }


        }
    }
}
