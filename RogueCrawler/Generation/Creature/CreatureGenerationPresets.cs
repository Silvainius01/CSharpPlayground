using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;
using static RogueCrawler.DungeonSettings;

namespace RogueCrawler
{
    class CreatureGenerationPresets
    {
        public static readonly Vector2Int LowHealthRange = new Vector2Int(MinCreatureHitPoints, LowCreatureHitPoints);
        public static readonly Vector2Int MidHealthRange = new Vector2Int(LowCreatureHitPoints, MidCreatureHitPoints);
        public static readonly Vector2Int HighHealthRange = new Vector2Int(MidCreatureHitPoints, MaxCreatureHitPoints);
        public static readonly Vector2Int RenownedHealthRange = new Vector2Int(MidCreatureHitPoints, MaxCreatureHitPoints+(MaxCreatureHitPoints/2));
        public static readonly Vector2Int LegendaryHealthRange = new Vector2Int(MidCreatureHitPoints, MaxCreatureHitPoints*2);
        public static readonly Vector2Int AnyHealthRange = new Vector2Int(MinCreatureHitPoints, MaxCreatureHitPoints);
        public static Vector2Int GetBaseHealthRange(QualityLevel level)
        {
            switch(level)
            {
                case QualityLevel.Low: return LowHealthRange;
                case QualityLevel.Normal: return MidHealthRange;
                case QualityLevel.Superior: return HighHealthRange;
                case QualityLevel.Exalted: return RenownedHealthRange;
                case QualityLevel.Legendary: return LegendaryHealthRange;
            }
            return AnyHealthRange;
        }

        public static readonly Vector2Int LowSkillRange = new Vector2Int(1, 25);
        public static readonly Vector2Int MidSkillRange = new Vector2Int(25, 40);
        public static readonly Vector2Int HighSkillRange = new Vector2Int(40, 60);
        public static readonly Vector2Int RenownedSkillRange = new Vector2Int(60, 90);
        public static readonly Vector2Int LegendarySkillRange = new Vector2Int(85, 100);
        public static readonly Vector2Int AnySkillRange = new Vector2Int(1, 100);
        public static Vector2Int GetBaseSkillRange(QualityLevel level)
        {
            switch (level)
            {
                case QualityLevel.Low: return LowSkillRange;
                case QualityLevel.Normal: return MidSkillRange;
                case QualityLevel.Superior: return HighSkillRange;
                case QualityLevel.Exalted: return RenownedSkillRange;
                case QualityLevel.Legendary: return LegendarySkillRange;
            }
            return AnySkillRange;
        }

        public static CreatureGenerationParameters RandomCreature
        {
            get
            {
                var q = DungeonChestGenerationPresets.GetRandomQuality();
                return new CreatureGenerationParameters(2, () => EnumExt<QualityLevel>.RandomValue)
                {
                    LevelRange = new Vector2Int(MinCreatureLevel, MaxCreatureLevel),
                    BaseHealthRange = AnyHealthRange,
                    BaseStatRange = new Vector2Int(MinCreatureAttributeScore, MaxCreatureAttributeScore),
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
