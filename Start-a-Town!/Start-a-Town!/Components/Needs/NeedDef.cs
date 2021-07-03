﻿using System;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public sealed class NeedDef : Def
    {
        public Type Type;
        public float BaseThreshold = 20;
        public float BaseDecayRate = .1f; // measure decay rate in ticks? how many ticks to drop value by 1
        public float BaseValue = 100;
        public TaskGiver TaskGiver;
        public NeedCategoryDef CategoryDef;
        public Need Create(Actor actor)
        {
            var n = Activator.CreateInstance(this.Type, actor) as Need;
            n.NeedDef = this;
            return n;
        }
        
        public NeedDef(string name, Type needType, NeedCategoryDef category = null) : base(name)
        {
            this.Type = needType;
            this.CategoryDef = category;
        }
        static public readonly NeedDef Comfort = new("Comfort", typeof(NeedComfort))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological,
            BaseDecayRate = 0, 
            BaseValue = 50
        };
        static public readonly NeedDef Hunger = new("Hunger", typeof(NeedFood))
        {
            TaskGiver = new TaskGiverEat(),
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological
        };
        static public readonly NeedDef Energy = new("Energy", typeof(NeedEnergy))
        {
            TaskGiver = new TaskGiverSleeping(),
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological
        };
        static public readonly NeedDef Work = new("Work", typeof(NeedWork))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryEsteem
        };
        static public readonly NeedDef Social = new("Social", typeof(NeedSocial))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryRelationships
        };

        static public readonly NeedDef Curiosity = new("Curiosity", typeof(NeedCuriosity))
        {
            CategoryDef = NeedCategoryDef.NeedCategoryCognitive
        };

        static NeedDef()
        {
            Register(Comfort);
            Register(Hunger);
            Register(Energy);
            Register(Work);
            Register(Social);
            Register(Curiosity);
        }
    }
}
