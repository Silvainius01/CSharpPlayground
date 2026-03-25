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
        public DamageType Type { get; set; }
        public Creature Attacker { get; set; }

        public DamageParameters(float amount, DamageType dType) 
        {
            Amount = amount;
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
        public float Received { get; set; }
        public bool DefenderDies { get; set; }
        public bool AttackSuccessful { get; set; }
        public Creature Defender { get; set; }
        public DamageParameters InitialParams { get; set; }

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
            if (DamageType != DamageType.True)
            {
                // Armor rating
                float ar = MathF.Floor(Defender.GetArmorRating());
                float total = BaseAmount * (BaseAmount / (2 * ar + BaseAmount));
                return Mathc.Truncate(total, 1);
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
