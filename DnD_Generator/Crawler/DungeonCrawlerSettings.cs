﻿using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    static class DungeonCrawlerSettings
    {
        #region Internal Settings
        public const string TabString = "  ";
        public const int CommandsPerCreatureAttack = 2;
        public const int CommandsPerCreatureHeal = 5;
        #endregion

        #region Player Settings
        /// <summary>Minimum HitPoints a player can have, regardless of attributes</summary>
        public const int MinPlayerHitPoints = 5;
        /// <summary>Most amount of levels generated loot can be below a player's level</summary>
        public const int PlayerRelativeLootFloor = 5;
        /// <summary>Most amount of levels generated loot can be above a player's level</summary>
        public const int PlayerRelativeLootCeiling = 2;
        public const int ExperiencePerExploredRoom = 5;
        public const int ExperiencePerCreatureKilled = 10;
        public const int ExperiencePerCreatureOverLevel = 5;
        public const float FullClearBonus = 2.0f;
        public const float FullExploreBonus = 2.0f;
        public const int LevelsPerDungeonSizeUnlock = 5;
        #endregion

        #region Attribute Settings
        /// <summary>Amount of attribute points gained per level</summary>
        public const int AttributePointsPerLevel = 2;
        /// <summary>Bonus HitPoints granted by every point of CON</summary>
        public const int HitPointsPerConstitution = 5;
        #endregion

        #region Global Generation Settings
        public const float QualityLevelShiftChance = 0.1f;
        #endregion

        #region Creature Generation Settings
        /// <summary>Minimum hitpoints a creature can have with Attribute level 0</summary>
        public const int MinCreatureHitPoints = 5;
        /// <summary>Maximum hitpoints a creature can have with Attribute Level 0</summary>
        public const int MaxCreatureHitPoints = 50;
        /// <summary></summary>
        public const int MidCreatureHitPoints = (int)((MaxCreatureHitPoints - MinCreatureHitPoints) * (2.0 / 3.0)) + MinCreatureHitPoints;
        /// <summary></summary>
        public const int LowCreatureHitPoints = (int)((MaxCreatureHitPoints - MinCreatureHitPoints) * (1.0 / 3.0)) + MinCreatureHitPoints;

        /// <summary>Minimum score a creature can have at Level 0</summary>
        public const int MinCreatureAttributeScore = 0;
        /// <summary>Maximum score a creature can have at level 0</summary>
        public const int MaxCreatureAttributeScore = 0;

        /// <summary>Minimum level a creature can be</summary>
        public const int MinCreatureLevel = 1;
        /// <summary>Maximum level a creature can be</summary>
        public const int MaxCreatureLevel = 100;
        #endregion

        #region Chest Generation Settings
        /// <summary>Loot level multiplier for Low Quality loot</summary>
        public const float LowQualityLootBias = 0.5f;
        /// <summary>Loot level multiplier for Mid Quality loot</summary>
        public const float MidQualityLootBias = 1.0f;
        /// <summary>Loot level multiplier for High Quality loot</summary>
        public const float HighQualityLootBias = 1.5f;
        #endregion

        #region Weapon Generation Settings
        /// <summary>Determines if weapons of quality level 0 can be generated</summary>
        public const bool AllowBrokenWeapons = true;
        /// <summary>The maximum quality level a weapon can be</summary>
        public const int MaxWeaponQuality = 50;
        /// <summary>The maximum quality a mid-range weapon can be</summary>
        public const int MidWeaponQuality = (int)(MaxWeaponQuality * (2.0 / 3.0));
        /// <summary>The maximum quality a low-range weapon can be</summary>
        public const int LowWeaponQuality = (int)(MaxWeaponQuality * (1.0 / 3.0));

        /// <summary>The most a weapon can weigh</summary>
        public const int MaxWeaponWeight = 100;
        /// <summary>The least a weapon can weight</summary>
        public const int MinWeaponWeight = 25;
        /// <summary>The most a mid-range weapon can weigh</summary>
        public const int MidWeaponWeight = (int)((MaxWeaponWeight - MinWeaponWeight) * (2.0 / 3.0)) + MinWeaponWeight;
        /// <summary>The most a low-range weapon can weigh</summary>
        public const int LowWeaponWeight = (int)((MaxWeaponWeight - MinWeaponWeight) * (1.0 / 3.0)) + MinWeaponWeight;
        /// <summary>Large weapon weight multiplier applied AFTER weapon type modifiers.</summary>
        public const float LargeWeaponWeightMultiplier = 1;
        #endregion
    }
}