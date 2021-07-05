using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using System;

namespace Start_a_Town_
{
    public class ActorDef : ItemDef
    {
        public NeedDef[] Needs;
        public AttributeDef[] Attributes;
        public ResourceDef[] Resources;
        public SkillDef[] Skills;
        public TraitDef[] Traits;
        public GearType[] GearSlots;

        public ActorDef(string name):base(name)
        {
            this.ItemClass = typeof(Actor);
        }
    }
}
