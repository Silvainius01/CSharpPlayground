using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    static class DungeonHelper
    {
        public static string GetQualityPrefix(float quality) => quality switch
        {
            <= 0 => DungeonConstants.QualityNameT0,  // Unusable
            < 1 => DungeonConstants.QualityNameT1,   // Less than base (0-1)
            < 3 => DungeonConstants.QualityNameT2,   // Base+ Damage (1-3) 
            < 7 => DungeonConstants.QualityNameT3,   // Double (3-7)
            < 15 => DungeonConstants.QualityNameT4,  // Triple (7-15)
            < 31 => DungeonConstants.QualityNameT5,  // Quadruple (15-31)
            <= 32 => DungeonConstants.QualityNameT6, // Pentuple (31-32)
            _ => DungeonConstants.QualityNameError   // Not possible
        };
    }
}
