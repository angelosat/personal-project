using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
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
        public override bool Condition(GameObject obj)
        {
            var mat = obj.Body.Material;
            if (mat != null)
                return obj.Body.Material.Fuel > 0;
            return false;
        }
    }
}
