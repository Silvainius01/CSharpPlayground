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
        protected const int minQualities = 2;

        public int CreatureLevel { get; set; } = 0;
        public Vector2Int WeightRange { get; set; }
        public Vector2Int QualityRange { get; set; } = new Vector2Int(-1, -1);

        public ItemMaterial Material { get; set; } = MaterialTypeManager.DefaultMaterial;

        public float QualityOverride = -1.0f;
        public QualityLevel Quality { get => Qualities[0]; }
        public QualityLevel QualityBias { get => Qualities[1]; }

        public ItemGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public ItemGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }
        public ItemGenerationParameters(QualityLevel quality) : base(minQualities, quality) { }
        public ItemGenerationParameters(Func<QualityLevel> DetermineQuality) : base(minQualities, DetermineQuality) { }

        protected override bool ValidateInternal()
        {
            WeightRange = Mathc.Clamp(WeightRange, ItemWeaponGenerationPresets.AnyWeight);
            QualityRange = Mathc.Max(QualityRange, 0);
            if (Qualities.Count < minQualities)
            {
                ConsoleExt.WriteWarningLine("Item didnt have needed quality entries. Filling with Normal");
                AddQuality(QualityLevel.Normal, minQualities - Qualities.Count);
            }
            return true;
        }
    }
}
