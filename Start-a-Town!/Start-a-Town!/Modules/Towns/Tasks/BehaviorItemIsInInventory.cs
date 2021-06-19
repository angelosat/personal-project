using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class BehaviorItemIsInInventory : Behavior
    {
        //private readonly TargetArgs Item;
        private readonly int TargetInd;

        public override object Clone()
        {
            throw new Exception();
            //return new BehaviorItemIsInInventory(this.Item);
        }
        public BehaviorItemIsInInventory(TargetIndex targetInd) : this((int)targetInd)
        {

        }
        public BehaviorItemIsInInventory(TargetArgs item)
        {
            throw new Exception();
            //this.Item = item;
        }
        public BehaviorItemIsInInventory(int targetInd)
        {
            this.TargetInd = targetInd;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var item = parent.CurrentTask.GetTarget(this.TargetInd);
            return parent.InventoryContains(item.Object) ? BehaviorState.Success : BehaviorState.Fail;

            //return parent.InventoryContains(this.Item.Object) ? BehaviorState.Success : BehaviorState.Fail;
        }
    }
}
