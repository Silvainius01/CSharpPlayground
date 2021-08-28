using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    enum DungeonSize { Small, Medium, Large, Huge }
    enum CreatureType { Humanoid, Goblinoid, Beast, Fey }
    enum Allignment { LawfulGood, LawfulNeutral, LawfulEvil, NeutralGood, TrueNeutral, NeutralEvil, ChaoticGood, ChaoticNeutral, ChoaticEvil }
    enum AttributeType { STR, DEX, CON, INT, WIS, CHA }
    enum Direction { North, East, South, West }
    enum DungeonChestType { Weapon, Armor, Misc, Any }
    enum QualityLevel { Low, Mid, High }
    enum ItemWeaponLargeRate { None, Low, Mid, High, All }
    enum ItemArmorSlotType { Head, Body, Legs, Feet, Ring }
    enum WeaponType { Blade, Ranged, Axe, Blunt }
}
