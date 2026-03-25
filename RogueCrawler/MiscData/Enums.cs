using System;
using System.Collections.Generic;
using System.Text;
using CommandEngine;

namespace RogueCrawler
{
    enum DungeonSize { Small, Medium, Large, Huge }
    enum CreatureType { Humanoid, Goblinoid, Beast, Fey, Demon, Angel }
    enum Allignment { LawfulGood, LawfulNeutral, LawfulEvil, NeutralGood, TrueNeutral, NeutralEvil, ChaoticGood, ChaoticNeutral, ChoaticEvil }
    enum AttributeType { STR, DEX, CON, INT, WIS, CHA }
    enum Direction { North, East, South, West }
    enum DungeonChestType { Weapon, Armor }
    enum QualityLevel { Low, Normal, Superior, Exalted, Legendary, Divine }
    enum ItemWeaponLargeRate { None, Low, Mid, High, All }
    enum ArmorSlotType { Head, Chest, Arm, Hand, Waist, Foot }
    enum AccessorySlotType { Ring, Necklace }
    enum ItemWeaponHandedness { Both, One, Two }
    enum DamageType { Physical, True }
    // enum WeaponType { Blade, Ranged, Axe, Blunt }
}
