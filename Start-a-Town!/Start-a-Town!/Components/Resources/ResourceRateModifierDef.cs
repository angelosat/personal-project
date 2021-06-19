using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class ResourceRateModifierDef : Def
    {
        public enum Types { Permanent, Finite }
        public Types Type;
        Func<GameObject, float> Function;
        //public delegate float Function(GameObject obj);
        public ResourceDef Source;
        public int BaseDurationInTicks = 0;
        public ResourceRateModifierDef(string name, ResourceDef source) : base(name)
        {
           
            this.Source = source;
        }
        public float GetRateMod(GameObject parent)
        {
            return this.Function(parent);
        }
        static public readonly ResourceRateModifierDef HaulingStaminaDrain = new ResourceRateModifierDef("HaulingStaminaDrain", ResourceDef.Stamina) { 
            Function = 
                (actor) =>
                {
                    //var w = actor.GetHauled()?.TotalWeight ?? 0;
                    //if (w == 0)
                    //    return 0;
                    //var maxw = StatNewDef.MaxHaulWeight.GetValue(actor);
                    //var val = 2 - (maxw / w);
                    //val *= .5f;// 0.1f;

                    var val = StatDefOf.Encumberance.GetValue(actor);
                    var factor = .1f;
                    val *= factor;
                    return -val;
                },
            Type = Types.Permanent
        };
    }
}
