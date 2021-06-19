using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Crafting
{
    class IsEdible : Reaction.Reagent.ReagentFilter
    {
        public override string Name
        {
            get
            {
                return "Is Edible";
            }
        }
        public IsEdible()
        {
        }
        public override bool Condition(Entity obj)
        {
            return obj.IDType == GameObject.Types.Berries;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
