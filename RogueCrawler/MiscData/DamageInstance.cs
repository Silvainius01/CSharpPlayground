using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    internal struct DamageParameters
    {
        public float Amount { get; set; }
        public DamageTypeData TypeData { get; set; }
        public Creature Attacker { get; set; }

        public DamageParameters(float amount, DamageTypeData dType)
        {
            Amount = Mathc.Truncate(amount, 1);
            TypeData = dType;
            Attacker = null;
        }
        public DamageParameters(Creature attacker)
        {
            Amount = attacker.GetCombatDamage();
            TypeData = attacker.GetDamageType();
            Attacker = attacker;
        }
    }

    internal struct DamageInstance : IInspectable
    {
        public float Received { get; private set; }
        public bool DefenderDies { get; private set; }
        public bool AttackSuccessful { get; set; }
        public Creature Defender { get; private set; }
        public DamageParameters InitialParams { get; private set; }

        public float ArmorReduction { get; private set; }
        public float ResistanceReduction { get; private set; }

        public float BaseAmount => InitialParams.Amount;
        public float TotalReduction => BaseAmount - Received;
        public DamageTypeData TypeData => InitialParams.TypeData;
        public Creature Attacker => InitialParams.Attacker;

        public DamageInstance(Creature attacker, Creature defender)
        {
            InitialParams = new DamageParameters(attacker);
            Defender = defender;
            Received = CalculateReceived();
            DefenderDies = Received >= defender.Health.Value;
        }
        public DamageInstance(DamageParameters dParams, Creature defender)
        {
            InitialParams = dParams;
            Defender = defender;
            Received = CalculateReceived();
            DefenderDies = Received >= defender.Health.Value;
        }

        private float CalculateReceived()
        {
            float damage = BaseAmount;
            DamageFlags dFlags = TypeData.Flags;

            // True Damage cannot be resisted, and isnt affected by armor.
            if (TypeData.Category != DamageCategory.True && dFlags != DamageFlags.True)
            {
                // Armor rating.
                // Armor comes first since if it didnt, players would be practically invincible once decked out in resistance buffs.
                if (dFlags.HasFlag(DamageFlags.IsBlockable))
                {
                    float ar = MathF.Floor(Defender.GetArmorRating());
                    ar = damage * (damage / (2 * ar + damage));
                    ArmorReduction = Mathc.Truncate(damage - ar, 1);
                    damage -= ArmorReduction;
                }

                // Resistance
                if (dFlags.HasFlag(DamageFlags.IsResistable))
                {
                    float resist = Defender.GetTypeResistance(TypeData.Name);
                    resist = damage * (1.0f / MathF.Pow(2.0f, resist));
                    ResistanceReduction = Mathc.Truncate(damage - resist, 1);
                    damage -= ResistanceReduction;
                }

                return damage;
            }
            return BaseAmount;
        }

        public string BriefString()
        {
            return $"{BaseAmount.ToString("n1")}, {EnumExt<DamageCategory>.GetName(TypeData.Category)}";
        }
        public string InspectString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
        public string DebugString(string prefix, int tabCount)
        {
            throw new NotImplementedException();
        }
    }
}
