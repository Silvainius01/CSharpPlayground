using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieRoller
{
    public class DiceRoll : IDiceRoll
    {
        public int NumDice { get; set; }
        public int NumSides { get; set; }

        public DiceRoll(int numDice, int numSides)
        {
            this.NumDice = numDice;
            this.NumSides = numSides;
        }
        public DiceRoll((int NumDice, int NumSides) diceRoll) : this(diceRoll.NumDice, diceRoll.NumSides) { }

        public int Roll() => DiceRoller.RollDice(NumDice, NumSides);

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
            return avg * NumDice;
        }
        public (int min, int max) GetRange()
        {
            return (NumDice, NumDice * NumSides);
        }
        
        public IEnumerable<int> GetDieSides()
        {
            for (int i = 0; i < NumSides; ++i)
                yield return i + 1;
        }

        public static bool TryParse(string str, out DiceRoll diceRoll)
        {
            string[] values = str.Split('d');

            if (values.Length != 2)
            {
                diceRoll = new DiceRoll(0, 0);
                return false;
            }

            bool success = int.TryParse(values[0], out int numDice);
            success &= int.TryParse(values[1], out int numSides);
            diceRoll = new DiceRoll(numDice, numSides);
            return
                success &&
                numDice > 0 &&
                numSides > 0;
        }

        public (IList<int> Rolls, int Total) RollSeperate()
        {
            List<int> rolls = new List<int>();
            
            for (int i = 0; i < NumDice; ++i)
            {
                int r = DiceRoller.RollDice(1, NumSides);
                rolls.Add(r);
            }

            return (rolls, rolls.Sum());
        }
    }
}
