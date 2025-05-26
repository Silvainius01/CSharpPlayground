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
    class ItemGenerationParameters : BaseGenerationParameters
    {
        public int CreatureLevel { get; set; } = 0;
        public Vector2Int WeightRange { get; set; }
        public Vector2Int QualityRange { get; set; } = new Vector2Int(-1, -1);

        public QualityLevel Quality { get => Qualities[0]; }
        public QualityLevel QualityBias { get => Qualities[0]; }

        public ItemGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public ItemGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }
        public ItemGenerationParameters(int numQualities, QualityLevel quality) : base(numQualities, quality) { }
        public ItemGenerationParameters(int numQualities, Func<QualityLevel> DetermineQuality) : base(numQualities, DetermineQuality) { }

        protected override bool ValidateInternal()
        {
            WeightRange = Mathc.Clamp(WeightRange, ItemWeaponGenerationPresets.AnyWeight);
            QualityRange = Mathc.Max(QualityRange, 0);
            return true;
        }
    }
}
