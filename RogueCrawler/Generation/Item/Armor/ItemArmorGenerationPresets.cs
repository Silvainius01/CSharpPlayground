using CommandEngine;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using static RogueCrawler.DungeonCrawlerSettings;

namespace RogueCrawler
{
    
    class ItemArmorGenerationPresets
    {        
        public static ItemArmorGenerationParameters GetParamsForChest(int level, QualityLevel quality)
        {
            return new ItemArmorGenerationParameters(quality, DungeonGenerator.GetRandomQuality())
            {
                CreatureLevel = level,
                PossibleArmorClasses = new List<string>()
                {
                    DungeonConstants.ArmorClassLight,
                    DungeonConstants.ArmorClassMedium,
                    DungeonConstants.ArmorClassHeavy,
                },
                PossibleArmorSlots = new List<ArmorSlotType>(EnumExt<ArmorSlotType>.Values)
            };
        }
        public static ItemArmorGenerationParameters GetParamsForCreature(Creature creature, QualityLevel weaponQuality, QualityLevel weightQuality)
        {
            return new ItemArmorGenerationParameters(weaponQuality, DungeonGenerator.GetRandomQuality())
            {
                CreatureLevel = creature.Level,
                PossibleArmorClasses = new List<string>()
                {
                    DungeonConstants.ArmorClassLight,
                    DungeonConstants.ArmorClassMedium,
                    DungeonConstants.ArmorClassHeavy,
                },
                PossibleArmorSlots = new List<ArmorSlotType>(EnumExt<ArmorSlotType>.Values)
            };
        }
    }
}

