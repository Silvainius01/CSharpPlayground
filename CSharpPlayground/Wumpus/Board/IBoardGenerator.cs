using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandEngine;

namespace CSharpPlayground.Wumpus
{
    public interface IBoardGenerator
    {
        List<BoardRoom> rooms { get; set; }

        /// <summary>
        /// Creates and returns a list of interconnected rooms for use as a GameBoard
        /// </summary>
        void GenerateBoard();
    }
}
