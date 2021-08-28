using System;
using System.Collections.Generic;
using System.Text;
using GameEngine;

namespace DnD_Generator
{
    /// <summary>
    /// Required Qualities:
    /// <para>ItemQuality, ItemWeight</para>
    /// </summary>
    class DungeonChestGenerationParamerters : BaseGenerationParameters
    {
        public int CreatureLevel { get; set; }
        public DungeonChestType ChestType { get; set; }
        public Vector2Int ItemRange { get; set; }

        public QualityLevel ItemQuality { get => Qualities[0]; }
        public QualityLevel ItemWeight { get => Qualities[1]; }

        public DungeonChestGenerationParamerters(int numQualities, QualityLevel quality) : base(numQualities, quality) { }
        public DungeonChestGenerationParamerters(int numQualities, Func<QualityLevel> DetermineQuality) : base(numQualities, DetermineQuality) { }
        public DungeonChestGenerationParamerters(IEnumerable<QualityLevel> qualities) : base(qualities) { }
        public DungeonChestGenerationParamerters(params QualityLevel[] qualities) : base(qualities) { }

        protected override bool ValidateInternal()
        {
            ItemRange = Mathc.Min(ItemRange.Sort(), 0);
            return true;
        }
    }

    class DungeonChestGenerator : DungeonObjectGenerator<DungeonChest<IItem>, DungeonChestGenerationParamerters, DungeonChestManager>
    {
        public override DungeonChestManager GenerateObjects(DungeonGenerationParameters dParams, DungeonRoomManager roomManager)
        {
            dParams.Validate();

            DungeonChestManager chestManager = new DungeonChestManager(roomManager);
            foreach (var room in roomManager.rooms)
            {
                if (room.Index != roomManager.EntranceRoom.Index && Mathc.Random.NextFloat() < dParams.ChestProbability)
                {
                    DungeonChestGenerationParamerters cParams = new DungeonChestGenerationParamerters(2, () => EnumExt<QualityLevel>.RandomValue)
                    {
                        ChestType = DungeonChestType.Weapon,
                        ItemRange = new Vector2Int(2, 5)
                    };
                    chestManager.AddObject(DungeonGenerator.GenerateChest(cParams), room);
                }
            }
            return chestManager;
        }

        public override DungeonChest<IItem> Generate(DungeonChestGenerationParamerters cParams)
        {
            DungeonChest<IItem> chest = new DungeonChest<IItem>()
            {
                ID = NextId,
                Type = DungeonChestType.Weapon
            };

            // Assume a wepaons chest for now
            chest.Name = PopulateWeaponsChest(chest, cParams);

            return chest;
        }

        public static DungeonChest<IItem> GetEmptyChest() => new DungeonChest<IItem>() { ID = NextId };

        static string PopulateWeaponsChest(DungeonChest<IItem> chest, DungeonChestGenerationParamerters cParams)
        {
            int numItems = Mathc.Random.NextInt(cParams.ItemRange, true);
            for (int i = 0; i < numItems; ++i)
            {
                ItemWeaponGenerationParameters weaponProperties =
                    ItemWeaponGenerationPresets.GetParamsForChest(cParams.CreatureLevel, cParams.ItemQuality, cParams.ItemWeight);
                chest.AddItem(DungeonGenerator.GenerateWeapon(weaponProperties));
            }
            return $"{cParams.ItemQuality}Quality {cParams.ItemWeight}Weight WeaponChest";
        }
    }
}
