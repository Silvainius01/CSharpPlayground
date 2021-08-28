using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    /// <summary>
    /// Required Qualities:
    /// <para>QualityBias</para>
    /// </summary>
    class ItemWeaponGenerationParameters : ItemGenerationParameters
    {   
        public bool GenerateRelative { get; set; }
        public bool CapToCreatureLevel { get; set; }
        public int LargeWeaponProbability { get; set; } = 50;
        public List<WeaponType> PossibleWeaponTypes { get; set; } = new List<WeaponType>();

        public ItemWeaponGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public ItemWeaponGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }
        public ItemWeaponGenerationParameters(int numQualities, QualityLevel quality) : base(numQualities, quality) { }
        public ItemWeaponGenerationParameters(int numQualities, Func<QualityLevel> DetermineQuality) : base(numQualities, DetermineQuality) { }
    }
}
