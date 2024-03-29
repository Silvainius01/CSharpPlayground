﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CommandEngine;

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
            float bias = 1.0f;
            switch (quality)
            {
                case QualityLevel.Low: bias = DungeonCrawlerSettings.LowQualityLootLevelBias; break;
                case QualityLevel.Mid: bias = DungeonCrawlerSettings.MidQualityLootLevelBias; break;
                case QualityLevel.High: bias = DungeonCrawlerSettings.HighQualityLootLevelBias; break;
                case QualityLevel.Renowned: bias = DungeonCrawlerSettings.RenownedQualityLootLevelBias; break;
                case QualityLevel.Legendary: bias = DungeonCrawlerSettings.LegendaryQualityLootLevelBias; break;
            }
            return bias;
        }
        public static int GetRandomRelativeLevel(int level, QualityLevel quality)
        {
            return GetRandomRelativeLevel(level, GetLevelBias(quality));
        }
        public static int GetRandomRelativeLevel(int level, float bias = 1.0f)
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

        /// <summary>Rolls against a normal curve to see if the quality gets shift up or down</summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static QualityLevel GetShiftedQuality(QualityLevel level)
        {
            float rFloat = CommandEngine.Random.NextFloat();
            if (rFloat < DungeonCrawlerSettings.QualityLevelShiftChance)
            {
                // Increase or decrease level by one based on second decimal place
                int shiftedLevel = (int)level + ((int)(rFloat * 100) % 2 == 0 ? 1 : -1);
                return (QualityLevel)Mathc.Mod(shiftedLevel, EnumExt<QualityLevel>.Count);
            }
            return level;
        }
    }
}
