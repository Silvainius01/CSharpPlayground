using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;
using System.Linq;

namespace RogueCrawler
{
    
    static class DungeonChestGenerationPresets
    {
        public static QualityLevel GetRandomQuality()
            => EnumExt<QualityLevel>.RandomValue;
        public static QualityLevel GetQualityOrHigher(QualityLevel lootGrade) 
            => (QualityLevel)CommandEngine.Random.NextInt((int)lootGrade, EnumExt<QualityLevel>.Count);
        public static QualityLevel GetQualityOrLower(QualityLevel lootGrade) 
            => (QualityLevel)CommandEngine.Random.NextInt((int)lootGrade, true);

        public static DungeonChestType GetRandomType() 
            => EnumExt<DungeonChestType>.RandomValue;
    }
}
