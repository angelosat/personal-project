using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Crafting
{
    class IsFuel : Reaction.Reagent.ReagentFilter
    {
        public override string Name
        {
            get
            {
                return "Is fuel";
            }
        }
        public override bool Condition(Entity obj)
        {
            var mat = obj.Body.Material;
            if (mat != null)
                return obj.Body.Material.Fuel.Value > 0;
            return false;
        }
    }
}
