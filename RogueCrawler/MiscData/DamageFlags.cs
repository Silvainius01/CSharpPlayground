using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{
    [Flags]
    internal enum DamageFlags
    {
        True = 0,
        Blockable = 1, // If a damage type is blockable, it can be mitigated by armor.
        Resistable = 2,   
        IsElemental = 4,
        IsArcane = 8,
        IsDivine = 16
    }
    internal struct DamageParameters
    {
        public float Amount { get; set; }
        public DamageType Type { get; set; }
        public int DamageArchetype { get; set; }
        public Creature Attacker { get; set; }

        public DamageParameters(float amount, DamageType dType) 
        {
            Amount = Mathc.Truncate(amount, 1);
            Type = dType;
            Attacker = null;
        }
        public DamageParameters(Creature attacker)
        {
            Amount = attacker.GetCombatDamage();
            Type = attacker.GetDamageType();
            Attacker = attacker;
        }
    }
    internal struct DamageInstance : IInspectable
    {
        public float Received { get; private set; }
        public bool DefenderDies { get; private set; }
        public bool AttackSuccessful { get; private set; }
        public Creature Defender { get; private set; }
        public DamageParameters InitialParams { get; private set; }

        public float ArmorReduction { get; private set; }
        public float ResistanceReduction { get; private set; }

        public float BaseAmount => InitialParams.Amount;
        public float TotalReduction => BaseAmount - Received;
        public DamageType DamageType => InitialParams.Type;
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
            DamageType dType = DamageType;

            // True Damage cannot be resisted, and isnt affected by armor.
            if (dType != DamageType.True)
            {
                // Resistance
                float resist = Defender.GetResistance(dType);
                resist = damage * (1.0f / MathF.Pow(2.0f, resist));
                ResistanceReduction = Mathc.Truncate(resist, 1);

                // Armor rating
                float ar = MathF.Floor(Defender.GetArmorRating());
                ar = damage * (damage / (2 * ar + damage));
                ArmorReduction = Mathc.Truncate(damage - ar, 1);

                return ;
            }
            return BaseAmount;
        }

        public string BriefString()
        {
            return $"{BaseAmount.ToString("n1")}, {EnumExt<DamageType>.GetName(DamageType)}";
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
