using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class InteractionSwitch : Interaction
    {
        public override void Perform(GameObject a, TargetArgs t)
        {
            var e = a.Map.GetBlockEntity(t.Global);
            e.GetComp<BlockEntityCompSwitchable>().Toggle(a, t);
            this.Finish(a, t);
        }
        public override object Clone()
        {
            return new InteractionSwitch();
        }
    }
}
