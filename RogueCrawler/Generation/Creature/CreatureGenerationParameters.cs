using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    /// <summary>
    /// Required Qualities:
    /// <para>WeaponQuality, WeaponWeight</para>
    /// </summary>
    class CreatureGenerationParameters : BaseGenerationParameters
    {
        public Vector2Int BaseHealthRange { get; set; }
        public Vector2Int LevelRange { get; set; }
        public Vector2Int BaseStatRange { get; set; } = Vector2Int.Zero;
        public Vector2Int BaseProfiecienyRange { get; set; }
        public float WeaponChance { get; set; }
        
        public QualityLevel WeaponQuality { get => Qualities[0]; }
        public QualityLevel WeaponWeight { get => Qualities[1]; }
        public QualityLevel CreatureDifficulty { get => Qualities[2]; }

        public CreatureGenerationParameters(int numQualities, QualityLevel quality) : base(numQualities, quality) { }
        public CreatureGenerationParameters(int numQualities, Func<QualityLevel> DetermineQuality) : base(numQualities, DetermineQuality) { }
        public CreatureGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public CreatureGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }

        protected override bool ValidateInternal()
        {
            QualityLevel[] rLevel = new QualityLevel[] { QualityLevel.Low, QualityLevel.Mid };

            while (Qualities.Count < 3)
                Qualities.Add(rLevel.RandomItem());

            LevelRange = Mathc.Max(LevelRange.Sort(), 1);
            BaseStatRange = Mathc.Max(BaseStatRange.Sort(), 0);
            BaseHealthRange = Mathc.Max(BaseHealthRange.Sort(), 0);
            BaseProfiecienyRange = Mathc.Max(BaseProfiecienyRange.Sort(), 0);
            return true;
        }
    }
}
