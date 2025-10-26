using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;
using Microsoft.VisualBasic;

namespace RogueCrawler
{
    class DungeonGenerator
    { 
        public static Dungeon GenerateDungeon(DungeonGenerationParameters dParams)
        {
            dParams.Validate();
            DungeonRoomManager roomManager = CreateRoomManager(dParams);
            Dungeon dungeon = new Dungeon(
                roomManager,
                CreateCreatureManager(dParams, roomManager),
                CreateChestManager(dParams, roomManager)
            );
            dungeon.dParams = dParams;
            return dungeon;
        }

        static ItemWeaponGenerator itemWeaponGenerator = new ItemWeaponGenerator();
        public static ItemWeapon GenerateWeapon(ItemWeaponGenerationParameters wParams) 
            => itemWeaponGenerator.Generate(wParams);
        public static ItemWeapon GenerateWeaponFromSerialized(SerializedWeapon serialized)
            => itemWeaponGenerator.FromSerializable(serialized);
        public static ItemWeapon GenerateUnarmedWeapon(Creature c)
            => itemWeaponGenerator.GenerateUnarmed(c);

        static ItemArmorGenerator itemArmorGenerator = new ItemArmorGenerator();
        public static ItemArmor GenerateArmorFromSerialized(SerializedArmor serialized)
            => itemArmorGenerator.FromSerializable(serialized);
        public static ItemArmor GenerateUnarmoredSlot(ArmorSlotType armorSlotType)
            => itemArmorGenerator.GenerateUnarmoredSlot(armorSlotType);

        static DungeonRoomGenerator roomGenerator = new DungeonRoomGenerator();
        public static DungeonRoomManager CreateRoomManager(DungeonGenerationParameters dParams) 
            => roomGenerator.GenerateDungeonRooms(dParams);

        static CreatureGenerator creatureGenerator = new CreatureGenerator();
        public static Creature GenerateCreature(CreatureGenerationParameters cParams) 
            => creatureGenerator.Generate(cParams);
        public static DungeonCreatureManager CreateCreatureManager(DungeonGenerationParameters dParams, DungeonRoomManager roomManager)
            => creatureGenerator.GenerateObjects(dParams, roomManager);

        static DungeonChestGenerator chestGenerator = new DungeonChestGenerator();
        public static DungeonChest<IItem> GenerateChest(DungeonChestGenerationParamerters cParams)
            => chestGenerator.Generate(cParams);
        public static DungeonChestManager CreateChestManager(DungeonGenerationParameters dParams, DungeonRoomManager roomManager)
            => chestGenerator.GenerateObjects(dParams, roomManager);

        public static float GetLevelBias(QualityLevel quality)
        {
            float bias = 0.0f;
            switch (quality)
            {
                case QualityLevel.Low: bias = DungeonCrawlerSettings.LowQualityLevelBias; break;
                case QualityLevel.Normal: bias = DungeonCrawlerSettings.NormalLevelBias; break;
                case QualityLevel.Superior: bias = DungeonCrawlerSettings.SuperiorLevelBias; break;
                case QualityLevel.Exalted: bias = DungeonCrawlerSettings.ExaltedLevelBias; break;
                case QualityLevel.Legendary: bias = DungeonCrawlerSettings.LegendaryLevelBias; break;
            }
            return bias;
        }
        public static int GetRandomRelativeLevel(int level, QualityLevel quality)
        {
            return GetRandomRelativeLevel(level, GetLevelBias(quality));
        }
        public static int GetRandomRelativeLevel(int level, float bias = 0.0f)
        {
            var rDouble = CommandEngine.Random.GetMarsagliaBetween(
                RelativeLootLevelFloor(level),
                RelativeLootLevelCeiling(level));
            return Mathc.Clamp((int)Math.Ceiling(rDouble + bias), DungeonCrawlerSettings.MinCreatureLevel, DungeonCrawlerSettings.MaxCreatureLevel);
        }

        public static Vector2Int GetRelativeLootRange(int level)
            => new Vector2Int(RelativeLootLevelFloor(level), RelativeLootLevelCeiling(level));
        public static int RelativeLootLevelFloor(int level)
            => Mathc.Max(level - DungeonCrawlerSettings.PlayerRelativeLootFloor, DungeonCrawlerSettings.MinCreatureLevel);
        public static int RelativeLootLevelCeiling(int level)
            => Mathc.Min(level + DungeonCrawlerSettings.PlayerRelativeLootCeiling, DungeonCrawlerSettings.MaxCreatureLevel);

        /// <summary>Returns a random quality on a predetermined distribution</summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static QualityLevel GetRandomQuality() => CommandEngine.Random.NextFloat() switch
        {
            <= 0.2f => QualityLevel.Low,
            <= 0.7f => QualityLevel.Normal,
            <= 0.9f => QualityLevel.Superior,
            <= 0.975f => QualityLevel.Exalted,
            _ => QualityLevel.Legendary
        };
        public static Vector2Int GetBaseQualityRange(QualityLevel quality)
        {
            int Qmax = (2 << (int)quality) - 1;
            int Qmin = (Qmax + 1) / 2 - 1;

            return new Vector2Int(Qmin, Qmax);
        }
        public static float GetItemQuality(QualityLevel baseQuality, QualityLevel qualityBias)
        {
            float bias = 0;
            float quality = 0;
            Vector2Int qRange = GetBaseQualityRange(baseQuality);

            switch(qualityBias)
            {
                case QualityLevel.Low:
                    bias = -0.25f;
                    break;
                case QualityLevel.Normal:
                    break;
                case QualityLevel.Superior:
                    bias = 0.25f;
                    break;
                case QualityLevel.Exalted:
                    bias = 0.25f;
                    qRange.Y += 1;
                    break;
                case QualityLevel.Legendary:
                    bias = 0.25f;
                    qRange.X += 1;
                    qRange.Y += 1;
                    break;
            }

            bias = (qRange.Y - qRange.X) / 2 * bias;
            quality = CommandEngine.Random.GetMarsagliaBetween(qRange) + bias;
            return Mathc.Clamp(quality, 0, 30);
        }
    }
}
