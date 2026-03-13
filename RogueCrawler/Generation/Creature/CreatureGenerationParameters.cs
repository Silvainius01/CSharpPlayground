using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    /// <summary>
    /// Required Qualities:
    /// <para>WeaponQuality, WeaponWeight, CreatureDifficulty, ArmorQuality</para>
    /// </summary>
    class CreatureGenerationParameters : BaseGenerationParameters
    {
        public Vector2Int BaseHealthRange { get; set; }
        public Vector2Int LevelRange { get; set; }
        public Vector2Int BaseStatRange { get; set; } = Vector2Int.Zero;
        public Vector2Int BaseProfiecienyRange { get; set; }
        public float WeaponChance { get; set; }

        public int MaxArmorPieces { get; set; } = -1;
        public float ArmorChance { get; set; } = -1;

        private const int numQualities = 4;
        public QualityLevel WeaponQuality { get => Qualities[0]; }
        public QualityLevel WeaponWeight { get => Qualities[1]; }
        public QualityLevel CreatureDifficulty { get => Qualities[2]; }
        public QualityLevel ArmorQuality { get => Qualities[3]; }

        public CreatureGenerationParameters(int numQualities, QualityLevel quality) : base(numQualities, quality) { }
        public CreatureGenerationParameters(int numQualities, Func<QualityLevel> DetermineQuality) : base(numQualities, DetermineQuality) { }
        public CreatureGenerationParameters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public CreatureGenerationParameters(params QualityLevel[] qualities) : base(qualities) { }


        protected override bool ValidateInternal()
        {
            QualityLevel[] rLevel = new QualityLevel[] { QualityLevel.Low, QualityLevel.Normal };

            while (Qualities.Count < numQualities)
                Qualities.Add(rLevel.RandomItem());

            LevelRange = Mathc.Max(LevelRange.Sort(), 1);
            BaseStatRange = Mathc.Max(BaseStatRange.Sort(), 0);
            BaseHealthRange = Mathc.Max(BaseHealthRange.Sort(), 0);

            // I hope this shit gets meme'd on by the code review YouTubers
            if (ArmorChance < 0)
            {
                ArmorChance = ((int)CreatureDifficulty + 1) / (float)EnumExt<QualityLevel>.Count;
            }

            if (MaxArmorPieces < 0)
            {
                int maxSlots = CreatureArmorSlots.TotalSlots;
                float percentArmor = ((int)CreatureDifficulty + 1) / (float)EnumExt<QualityLevel>.Count;
                MaxArmorPieces = (int)(percentArmor * maxSlots);
            }

            BaseProfiecienyRange = Mathc.Max(BaseProfiecienyRange.Sort(), 0);
            return true;
        }
    }
}
