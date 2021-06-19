using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        //public NeedDef(string name, NeedCategoryDef category = null) : base(name)
        //{
        //    this.CategoryDef = category;
        //}
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
            //BaseThreshold = 20,
            TaskGiver = new TaskGiverEat(),
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological
        };
        static public readonly NeedDef Energy = new("Energy", typeof(NeedEnergy))
        {
            //Name = ,
            //BaseThreshold = 20,
            TaskGiver = new TaskGiverSleeping(),
            CategoryDef = NeedCategoryDef.NeedCategoryPhysiological
        };
        static public readonly NeedDef Work = new("Work", typeof(NeedWork))
        {
            //Name = ,
            CategoryDef = NeedCategoryDef.NeedCategoryEsteem
            //BaseThreshold = 95,
            //TaskGiver = new TaskGiverSleeping()
        };
        static public readonly NeedDef Social = new("Social", typeof(NeedSocial))
        {
            //Name = ,
            CategoryDef = NeedCategoryDef.NeedCategoryRelationships
        };

        static public readonly NeedDef Curiosity = new("Curiosity", typeof(NeedCuriosity))
        {
            //Name = ,
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
