using CommandEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueCrawler
{

    internal struct DamageInstance : IInspectable
    {
        public float Amount { get; set; }
        public DamageType DamageType { get; set; }
        public Creature Attacker { get; set; }
        public Creature Defender { get; set; }
        public float TotalReduction { get; set; }

        public string BriefString()
        {
            return $"{Amount.ToString("n1")}, {EnumExt<DamageType>.GetName(DamageType)}";
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
