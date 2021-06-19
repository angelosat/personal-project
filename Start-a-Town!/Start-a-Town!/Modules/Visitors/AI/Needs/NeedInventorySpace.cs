using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class NeedInventorySpace : Need
    {
        public NeedInventorySpace(Actor parent) : base(parent)
        {
        }
        // TODO Move this to the def
        public override void Tick(GameObject parent)
        {
            var inv = parent.Inventory;
            //var p = inv.PercentageEmpty;
            //this.Value = p * p;
            var p = inv.PercentageFull;
            this.Value = 1 - p * p;
            this.Value *= 100;
        }

        //public override object Clone() => new NeedInventorySpace();
    }
}
