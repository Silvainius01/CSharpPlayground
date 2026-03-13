using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    /// <summary>
    /// Required Qualities:
    /// <para>Quality, QualityBias</para>
    /// </summary>
    class ItemArmorGenerationParameters : ItemGenerationParameters
    {
        public List<string> PossibleArmorClasses { get; set; } = new List<string>();
        public List<ArmorSlotType> PossibleArmorSlots { get; set; } = new List<ArmorSlotType>();

        public ItemArmorGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public ItemArmorGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }
        public ItemArmorGenerationParameters(QualityLevel quality) : base(quality) { }
        public ItemArmorGenerationParameters(Func<QualityLevel> DetermineQuality) : base(DetermineQuality) { }

        protected override bool ValidateInternal()
        {
            if (!base.ValidateInternal())
                return false;

            if (PossibleArmorSlots.Count == 0)
                PossibleArmorSlots.AddRange(EnumExt<ArmorSlotType>.Values);
            if (PossibleArmorClasses.Count == 0)
                PossibleArmorClasses.AddRange(ArmorTypeManager.ArmorByClass.Keys);

            return true;
        }
    }
}
