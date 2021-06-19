using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfType : Reaction.Reagent.ReagentFilter
    {
        ItemCategory Type;
        public override string Name
        {
            get
            {
                return "Is of type";
            }
        }
        public IsOfType(ItemCategory type)
        {
            this.Type = type;
        }
        public override bool Condition(GameObject obj)
        {
            return obj.GetInfo().ItemSubType.ItemType == this.Type;// obj.GetInfo().ID == this.Type;
        }
        public override string ToString()
        {
            //return Name + ": " + GameObject.Objects[this.Type].Name;
            return Name + ": " + this.Type;
        }
    }
}
