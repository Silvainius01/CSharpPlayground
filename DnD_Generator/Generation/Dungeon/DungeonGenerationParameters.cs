using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    /// <summary>
    /// Required Qualities:
    /// <para>AverageItemQuality, AverageItemWeight, CreatureDifficulty</para>
    /// </summary>
    class DungeonGenerationParameters : BaseGenerationParameters
    {
        public int PlayerLevel { get; set; }
        public Vector2Int ConnectionRange { get; set; }
        public Vector2Int RoomRange { get; set; }
        public Vector2Int RoomHeightRange { get; set; }
        public Vector2Int RoomWidthRange { get; set; }
        public int MaxCreaturesPerRoom { get; set; }
        public float CreatureProbability { get; set; }
        public float ChestProbability { get; set; }

        public QualityLevel AverageItemQuality { get => Qualities[0]; }
        public QualityLevel AverageItemWeight { get => Qualities[1]; }
        public QualityLevel CreatureDifficulty { get => Qualities[2]; }

        public DungeonGenerationParameters(int numQualities, QualityLevel quality) { AddQuality(quality, numQualities); }
        public DungeonGenerationParameters(int numQualities, Func<QualityLevel> DetermineQuality) { AddQuality(numQualities, DetermineQuality); }
        public DungeonGenerationParameters(IEnumerable<QualityLevel> qualities) { AddQuality(qualities); }
        public DungeonGenerationParameters(params QualityLevel[] qualities) { AddQuality(qualities); }

        protected override bool ValidateInternal()
        {
            RoomRange.X = Mathc.Max(2, RoomRange.X);
            RoomRange.Y = Mathc.Max(2, RoomRange.Y);

            ConnectionRange.SortSelf();
            RoomHeightRange.SortSelf();
            RoomWidthRange.SortSelf();

            return true;
        }
    }
}
