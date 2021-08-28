using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;
using System.Linq;

namespace DnD_Generator
{
    
    static class DungeonChestGenerationPresets
    {
        public static QualityLevel GetRandomQuality()
            => EnumExt<QualityLevel>.RandomValue;
        public static QualityLevel GetQualityOrHigher(QualityLevel lootGrade) 
            => (QualityLevel)Mathc.Random.NextInt((int)lootGrade, EnumExt<QualityLevel>.Count);
        public static QualityLevel GetQualityOrLower(QualityLevel lootGrade) 
            => (QualityLevel)Mathc.Random.NextInt((int)lootGrade, true);

        public static DungeonChestType GetRandomType() 
            => EnumExt<DungeonChestType>.RandomValue;
    }
}
