using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfType : Reaction.Reagent.ReagentFilter
    {
        ItemCategoryOld Type;
        public override string Name
        {
            get
            {
                return "Is of type";
            }
        }
        public IsOfType(ItemCategoryOld type)
        {
            this.Type = type;
        }
        public override bool Condition(Entity obj)
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
