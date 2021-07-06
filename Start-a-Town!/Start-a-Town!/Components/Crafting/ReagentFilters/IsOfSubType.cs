using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfSubType : Reaction.Reagent.ReagentFilter
    {
        //ItemSubType Type;
        List<ItemSubType> ValidTypes;

        public override string Name
        {
            get
            {
                return "Is of (any) subtype";
            }
        }
        //public IsOfSubType(ItemSubType type)
        //{
        //    this.Type = type;
        //}
        public IsOfSubType(params ItemSubType[] types)
        {
            this.ValidTypes = types.ToList(); ;
        }
        public override bool Condition(Entity obj)
        {
            //return obj.GetInfo().ItemSubType == this.Type;// obj.GetInfo().ID == this.Type;
            return this.ValidTypes.Contains(obj.GetInfo().ItemSubType);
        }
        public override string ToString()
        {
            //return Name + ": " + this.Type;
            string txt = Name + ": ";
            foreach (var type in this.ValidTypes)
                txt += type.Name + ", ";
            return txt.TrimEnd(',');
        }
    }
}
