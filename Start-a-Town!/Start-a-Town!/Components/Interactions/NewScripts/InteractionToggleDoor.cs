using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    public class InteractionToggleDoor : Interaction
    {
        public InteractionToggleDoor()
            : base("Open/close")
        {
            //this.Name = "Open/close";
            //this.Seconds = 0;
        }
        //static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
        //public override TaskConditions Conditions
        //{
        //    get
        //    {
        //        return conds;
        //    }
        //}
        internal override void InitAction(GameObject actor, TargetArgs target)
        {
            base.InitAction(actor, target);
            BlockDoor.Toggle(actor.Map, target.Global);
        }
        public override object Clone()
        {
            return new InteractionToggleDoor();
        }
    }
}
