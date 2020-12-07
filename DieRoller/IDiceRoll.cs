using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieRoller
{
    public interface IDiceRoll
    {
        int NumDice { get; set; }
        int NumSides { get; set; }

        int Roll();
        string GetStats();
        string GetFullStats();
        double GetAverage();
        (int min, int max) GetRange();
        IEnumerable<int> GetDieSides();
        (IList<int> Rolls, int Total) RollSeperate();
    }
}
