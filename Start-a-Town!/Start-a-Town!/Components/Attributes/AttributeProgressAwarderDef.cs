using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class AttributeProgressAwarderDef : Def
    {
        readonly Func<GameObject, float> Getter;
        public AttributeProgressAwarderDef(string name, Func<GameObject, float> getter) : base(name)
        {
     
            this.Getter = getter;
        }
        public float GetValue(GameObject a)
        {
            return this.Getter(a);
        }
        static public readonly AttributeProgressAwarderDef FromCarrying = new AttributeProgressAwarderDef("FromCarrying",
           a =>
           {
               var enc = StatDefOf.Encumberance.GetValue(a);
               return enc;
           });
    }
}
