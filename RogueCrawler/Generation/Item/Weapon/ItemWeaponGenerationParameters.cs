using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    /// <summary>
    /// Required Qualities:
    /// <para>QualityBias</para>
    /// </summary>
    class ItemWeaponGenerationParameters : ItemGenerationParameters
    {   
        public int LargeWeaponProbability { get; set; } = 25;
        public List<string> PossibleWeaponTypes { get; set; } = new List<string>();

        public ItemWeaponGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public ItemWeaponGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }
        public ItemWeaponGenerationParameters(QualityLevel quality) : base(quality) { }
        public ItemWeaponGenerationParameters(Func<QualityLevel> DetermineQuality) : base(DetermineQuality) { }
    }
}
