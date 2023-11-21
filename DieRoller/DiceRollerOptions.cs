using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieRoller
{
    public class DiceRollerMessageOptions
    {
        /// <summary>
        /// If true, displays the result of every die rolled
        /// </summary>
        public bool DisplayIndividualRolls { get; set; } = false;

        /// <summary>
        /// If true, displays the basic statistics for all rolls
        /// </summary>
        public bool DisplaySeperateStats { get; set; } = false;

        /// <summary>
        /// If true, displays the basic statistics for the combined roll
        /// </summary>
        public bool DisplayTotalStats { get; set; } = false;

        /// <summary>
        /// Display a full analysis of possible rolls, and their likelihood.
        /// </summary>
        public bool DisplayFullStats { get; set; } = false;

        /// <summary>
        /// If true, desiplays results for each set of dice seperately.
        /// </summary>
        public bool RollSeperate { get; set; } = false;

        public bool TakeHighest { get; set; } = false;
        public int NumHighest { get; set; } = 0;

        public bool TakeLowest { get; set; } = false;
        public int NumLowest { get; set; } = 0;

        public bool AllowReroll { get; set;} = false;
        public int RerollValue { get; set;} = 0;
        public int RerollAttempts { get; set; } = 0;
    }
}
