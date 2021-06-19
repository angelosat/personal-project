using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ReagentFilterItem
    {
        public ItemDef Specific;

        public ReagentFilterItem()
        {
        }

        public ReagentFilterItem(ItemDef itemDef)
        {
            this.Specific = itemDef;
        }

        public bool Condition(ItemDef def)
        {
            return this.Specific == null || def == this.Specific;
        }
    }
}