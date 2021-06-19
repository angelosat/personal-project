using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public enum DurationTypes { Permanent, Finite }
    class StatNewModifierDef : Def
    {
        public StatNewDef Source;
        //public List<Func<GameObject, float, float>> Modifiers = new List<Func<GameObject, float, float>>();
        public Func<GameObject, float, float> Mod;// = new List<Func<GameObject, float, float>>();
        public DurationTypes DurationType;
        //Func<GameObject, float, float> Function;
        public int BaseDurationInTicks = 0;
        public StatNewModifierDef(string name, StatNewDef source) : base(name)
        {
         
            this.Source = source;
        }

        static public readonly StatNewModifierDef WalkSpeedHaulingWeight = new StatNewModifierDef("WalkSpeedHaulingWeight", StatDefOf.WalkSpeed)
        {
            DurationType = DurationTypes.Permanent,
            Mod = (a,v)=>
            {
                var encumberance = StatDefOf.Encumberance.GetValue(a);
                var carryingw = a.GetHauled()?.TotalWeight ?? 0;
                if (carryingw == 0)
                    return v;
                return .5f + .5f * (1 - encumberance);
            }
        };
    }
}
