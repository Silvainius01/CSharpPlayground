using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandEngine;
using RogueCrawler.Item.Weapon;

namespace RogueCrawler
{
    class CreatureGenerator : DungeonObjectGenerator<Creature, CreatureGenerationParameters, DungeonCreatureManager>
    {
        public override DungeonCreatureManager GenerateObjects(DungeonGenerationParameters dParams, DungeonRoomManager roomManager)
        {
            dParams.Validate();

            DungeonCreatureManager creatureManager = new DungeonCreatureManager(roomManager);
            HashSet<DungeonRoom> validRooms = new HashSet<DungeonRoom>(roomManager.rooms, DungeonRoomGenerator.DungeonRoomEquality);
            CreatureGenerationParameters cParams = new CreatureGenerationParameters(dParams.AverageItemQuality, dParams.AverageItemWeight)
            {
                LevelRange = DungeonGenerator.GetRelativeLootRange(dParams.PlayerLevel),
                WeaponChance = 1.0f,
                BaseHealthRange = CreatureGenerationPresets.LowHealthRange
            };

            validRooms.Remove(roomManager.EntranceRoom);
            
            // Fill dungeon with the determined creature count
            for(int i = 0; i < roomManager.rooms.Count; ++i)
            {
                if(CommandEngine.Random.NextFloat() < dParams.CreatureProbability)
                {
                    DungeonRoom room = validRooms.RandomItem();
                    Creature creature = Generate(cParams);
                    creatureManager.AddObject(creature, room);

                    creature.CurrentRoom = room;
                    if (creatureManager.GetObjectCount(room) > dParams.MaxCreaturesPerRoom)
                        validRooms.Remove(room);
                }
            }
            
            return creatureManager;
        }

        public override Creature Generate(CreatureGenerationParameters cParams)
        {
            cParams.Validate();

            Creature creature = new Creature()
            {
                ID = NextId,
                ObjectName = "Gobbo",
                Level = CommandEngine.Random.NextInt(cParams.LevelRange)
            };

            creature.PrimaryWeapon = GenerateCreatureWeapon(cParams, creature);
            creature.AddAttributePoints(new CrawlerAttributeSet((attr) =>
            {
                int value = Math.Max(creature.PrimaryWeapon.AttributeRequirements[attr], DungeonCrawlerSettings.MinCreatureAttributeScore);
                if (value == 0 && attr == AttributeType.CON)
                    return 1; // Ensure CON is at least one
                return value;
            }));
            creature.Level = Math.Max(creature.MaxAttributes.CreatureLevel, creature.Level);

            //CrawlerAttributeSet attributes = creature.MaxAttributes;
            List <(AttributeType attribute, float chance)> attributeRanks = GetAttributeImportance(creature);
            int maxAttrScore = (creature.Level * DungeonCrawlerSettings.AttributePointsPerCreatureLevel);

            // Allocate remaining attribute points
            for (int i = creature.MaxAttributes.TotalScore; i < maxAttrScore; ++i)
            {
                bool applied = false;
                float rFloat = CommandEngine.Random.NextFloat();
                float currChance = 0.0f;

                for (int j = 0; j < attributeRanks.Count; j++)
                {
                    (AttributeType attribute, float chance) kvp = attributeRanks[j];
                    currChance += kvp.chance;
                    if (rFloat < currChance)
                    {
                        applied = true;
                        creature.AddAttributePoints(kvp.attribute, 1);
                        break;
                    }
                }

                // If for any reason no point was applied, apply it a completly random one.
                if (!applied)
                    creature.AddAttributePoints(EnumExt<AttributeType>.RandomValue, 1);
            }

            // Make sure all stats are full before spawning
            foreach (var stat in creature.Stats)
                stat.SetPercent(1.0f);

            return creature;
        }

        List<(AttributeType attribute, float chance)> GetAttributeImportance(Creature creature)
        {
            Dictionary<AttributeType, int> attributeRanks = new Dictionary<AttributeType, int>();
            bool AddWeaponStats(ItemWeapon w)
            {
                if (w == null)
                    return false;

                var weaponTypeData = WeaponTypeManager.WeaponTypes[w.WeaponType];
                attributeRanks[weaponTypeData.MajorAttribute] += 3;
                attributeRanks[weaponTypeData.MajorAttribute] += 2;
                return true;
            }

            foreach (var attr in EnumExt<AttributeType>.Values)
                attributeRanks.Add(attr, 0);

            // Add weapon stats to our rankings
            bool hasWeapon = false;
            hasWeapon |= AddWeaponStats(creature.PrimaryWeapon);
            hasWeapon |= AddWeaponStats(creature.SecondaryWeapon);

            // If unarmed, add STR and DEX of equal importance.
            if (!hasWeapon)
            {
                ++attributeRanks[AttributeType.STR];
                ++attributeRanks[AttributeType.DEX];
            }

            ++attributeRanks[AttributeType.STR]; // Add 1 to STR for being able to hold a weapon
            ++attributeRanks[AttributeType.CON]; // Add 1 to CON for HP

            float totalRankScore = attributeRanks.Aggregate(0.0f, (total, kvp) => total + kvp.Value);

            // 1) Filter out attributes with a score of 0
            // 2) Convert to list of tuples with the attributes chance of selection
            // 3) return
            return attributeRanks.Where((kvp) => kvp.Value > 0).ToList().ConvertAll((kvp) =>
            {
                (AttributeType attribute, float chance) = (kvp.Key, (float)kvp.Value / totalRankScore);
                return (attribute, chance);
            });
        }
        ItemWeapon GenerateCreatureWeapon(CreatureGenerationParameters cParams, Creature creature)
        {
            ItemWeapon weapon = null;
            if (CommandEngine.Random.NextFloat() < cParams.WeaponChance)
            {
                ItemWeaponGenerationParameters wParams =
                    ItemWeaponGenerationPresets.GetParamsForCreature(creature, cParams.WeaponQuality, cParams.WeaponWeight);
                weapon = DungeonGenerator.GenerateWeapon(wParams);
            }
            return weapon;
        }


        // Code for generating X creatures, with at most Y creatures per room.
        //void CreatureCountMethod()
        //{
        //    HashSet<DungeonRoom> validRooms = new HashSet<DungeonRoom>(roomManager.rooms, DungeonRoomGenerator.DungeonRoomEquality);

        //    validRooms.Remove(roomManager.EntranceRoom);
        //    for (int i = 0; i < totalCreatures && validRooms.Any();)
        //    {
        //        // Fill the room up to the randomly selected amount, or until the room is full.
        //        for (int j = 0; j < ObjectsInRoom && validRooms.Any(); ++i, ++j)
        //        {
        //            Creature c = Generate(cParams);
        //            DungeonRoom rRoom = validRooms.RandomItem();
        //            creatureManager.AddObject(c, rRoom);

        //            // Remove the room and stop adding creatures if the room is filled completely
        //            if (creatureManager.ObjectsInRoom(rRoom) >= dParams.RoomCreatureRange.Y)
        //            {
        //                validRooms.Remove(rRoom);
        //                break;
        //            }
        //        }
        //    }
        //}
    }
}