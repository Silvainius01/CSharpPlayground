using System;
using System.Collections.Generic;
using System.Text;

namespace DnD_Generator
{
    class PlayerCharacter : Creature
    {
        int Level { get; set; } = 1;
        int Experience { get; set; } = 0;
        int ExperienceNeeded { get; set; } = 100;
    }
}
