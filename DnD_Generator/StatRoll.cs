using DieRoller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnD_Generator
{
    class StatRoll : IDiceRoll
    {
        public int Floor { get; set; }
        public int NumDice { get; set; }
        public int NumSides { get; set; }

        /// <summary>
        /// Store a given roll set, with bonus
        /// </summary>
        /// <param name="numDice">Number of dice to be thrown when this roll is used</param>
        /// <param name="numSides">Number of sides per die</param>
        /// <param name="floor">Flat amount that is added to the roll result</param>
        public StatRoll(int numDice, int numSides, int floor)
        {
            this.NumDice = numDice;
            this.NumSides = numSides;
            this.Floor = floor;
        }
        public StatRoll(IDiceRoll diceRoll, int floor)
        {
            this.NumDice = diceRoll.NumDice;
            this.NumSides = diceRoll.NumSides;
            this.Floor = floor;
        }

        public int Roll() => DiceRoller.RollDice(NumDice, NumSides) + Floor;

        public string GetStats()
        {
            double avg = GetAverage();
            (int min, int max) range = GetRange();
            return ($"Avg: {avg}\nRange: {range.min}-{range.max}");
        }
        public string GetFullStats()
        {
            return DiceRoller.GetFullRollStats(this);
        }
        public double GetAverage()
        {
            double avg = (NumSides / 2) + 0.5;
            return (avg * NumDice) + Floor;
        }
        public (int min, int max) GetRange()
        {
            return (NumDice + Floor, (NumDice * NumSides) + Floor);
        }

        public IEnumerable<int> GetDieSides()
        {
            for (int i = 0; i < NumSides; ++i)
                yield return i + 1 + Floor;
        }

        public static bool TryParse(string str, out StatRoll statRoll)
        {
            string[] values = str.Split('d', '+');

            int floor = 0;
            int numDice = 0;
            int numSides = 0;
            bool success = true;
            switch(values.Length)
            {
                case 2:
                    success = int.TryParse(values[0], out numDice);
                    success &= int.TryParse(values[1], out numSides);
                    statRoll = new StatRoll(numDice, numSides, 0);
                    break;
                case 3:
                    success = int.TryParse(values[0], out numDice);
                    success &= int.TryParse(values[1], out numSides);
                    success &= int.TryParse(values[2], out floor);
                    statRoll = new StatRoll(numDice, numSides, floor);
                    break;
                default:
                    statRoll = new StatRoll(0, 0, 0);
                    return false;
            }

            return
                success &&
                numDice > 0 &&
                numSides > 0;
        }

        public (IEnumerable<int> Rolls, int Total) RollSeperate()
        {
            IList<int> rolls = new List<int>();

            for (int i = 0; i < NumDice; ++i)
            {
                int r = DiceRoller.RollDice(1, NumSides);
                rolls.Add(r);
            }

            return (rolls, rolls.Sum() + Floor);
        }

        (IList<int> Rolls, int Total) IDiceRoll.RollSeperate()
        {
            List<int> rolls = new List<int>();

            for (int i = 0; i < NumDice; ++i)
            {
                int r = DiceRoller.RollDice(1, NumSides);
                rolls.Add(r);
            }

            return (rolls, rolls.Sum() + Floor);
        }
    }
}
