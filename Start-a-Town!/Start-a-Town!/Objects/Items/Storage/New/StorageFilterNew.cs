using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class StorageFilterNew : ILabeled
    {
        public string Label { get; set; }
        public Predicate<Entity> Condition;
        public bool Enabled = true;

        public StorageFilterNew(string label, Predicate<Entity> condition)
        {
            Label = label;
            Condition = condition;
        }

        public StorageFilterNew(ItemDef item, Material mat, bool enabled = true)
        {
            this.Label = mat.Label + " " + item.Label;
            this.Condition = o => o.Def == item && o.PrimaryMaterial == mat;
            this.Enabled = enabled;
        }
        public StorageFilterNew(ItemDef item, bool enabled = true)
        {
            this.Label = item.Label;
            this.Condition = o => o.Def == item;
            this.Enabled = enabled;
        }
}
}
