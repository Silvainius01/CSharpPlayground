using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieRoller
{
    public class DiceRollSet : List<IDiceRoll>
    {
        public DiceRollSet() : base() { }
        public DiceRollSet(int capacity) : base(capacity) { }
        public DiceRollSet(IEnumerable<IDiceRoll> collection) : base(collection) { }

        public int Roll()
        {
            int result = 0;
            foreach (var dice in this)
                result += dice.Roll();
            return result;
        }

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
            double avg = 0;
            foreach (var diceRoll in this)
                avg += diceRoll.GetAverage();
            return avg;
        }
        public (int min, int max) GetRange()
        {
            (int min, int max) range = (0, 0);
            foreach (var diceRoll in this)
            {
                var r = diceRoll.GetRange();
                range = (range.min + r.min, range.max + r.max);
            }
            return range;
        }
    }
}
