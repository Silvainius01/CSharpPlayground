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
    class ItemArmorGenerationParameters : ItemGenerationParameters
    {   
        public List<string> PossibleArmorClasses { get; set; } = new List<string>();
        public List<ArmorSlotType> PossibleArmorSlots { get; set; } = new List<ArmorSlotType>();

        public ItemArmorGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public ItemArmorGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }
        public ItemArmorGenerationParameters(QualityLevel quality) : base(quality) { }
        public ItemArmorGenerationParameters(Func<QualityLevel> DetermineQuality) : base(DetermineQuality) { }
    }
}
