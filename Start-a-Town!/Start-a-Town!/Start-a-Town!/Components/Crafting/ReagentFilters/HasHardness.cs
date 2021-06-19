using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Components.Crafting
{
    class HasHardness : Reaction.Reagent.ReagentFilter
    {
        //public enum Comparisons { LessThan, EqualTo, GreaterThan };
        //Comparisons Comparison;

        float Min, Max;
      
        public override string Name
        {
            get
            {
                return "Has Hardness";
            }
        }
        //public HasHardness(Comparisons comparison, float hardness)
        //{
        //    this.Hardness = hardness;
        //}
        public HasHardness(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }
        public override bool Condition(GameObject obj)
        {
            var density = obj.GetComponent<MaterialsComponent>().Parts["Body"].Material.Density;
            return this.Min <= density && density <= this.Max;
        }
        public override string ToString()
        {
            return Name + ": " + this.Min + " <= x <= " + this.Max;
        }
    }
}
