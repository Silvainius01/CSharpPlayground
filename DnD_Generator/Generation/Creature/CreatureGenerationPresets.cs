using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;
using static DnD_Generator.DungeonCrawlerSettings;

namespace DnD_Generator
{
    class CreatureGenerationPresets
    {
        public static readonly Vector2Int LowHealthRange = new Vector2Int(MinCreatureHitPoints, LowCreatureHitPoints);
        public static readonly Vector2Int MidHealthRange = new Vector2Int(LowCreatureHitPoints, MidCreatureHitPoints);
        public static readonly Vector2Int HighHealthRange = new Vector2Int(MidCreatureHitPoints, MaxCreatureHitPoints);
        public static readonly Vector2Int AnyHealthRange = new Vector2Int(MinCreatureHitPoints, MaxCreatureHitPoints);
        public static Vector2Int GetBaseHealthRange(QualityLevel level)
        {
            switch(level)
            {
                case QualityLevel.Low: return LowHealthRange;
                case QualityLevel.Mid: return MidHealthRange;
                case QualityLevel.High: return HighHealthRange;
            }
            return AnyHealthRange;
        }

        public static CreatureGenerationParameters RandomCreature
        {
            get
            {
                var q = DungeonChestGenerationPresets.GetRandomQuality();
                return new CreatureGenerationParameters(2, () => EnumExt<QualityLevel>.RandomValue)
                {
                    LevelRange = new Vector2Int(DungeonCrawlerSettings.MinCreatureLevel, DungeonCrawlerSettings.MaxCreatureLevel),
                    BaseHealthRange = AnyHealthRange,
                    BaseStatRange = new Vector2Int(DungeonCrawlerSettings.MinCreatureAttributeScore, DungeonCrawlerSettings.MaxCreatureAttributeScore),
                    WeaponChance = 1.0f
                };
            }
        }
        public static CreatureGenerationParameters GetLeveledCreature(int level)
        {
            return new CreatureGenerationParameters()
            {

            };
        }
    }
}
