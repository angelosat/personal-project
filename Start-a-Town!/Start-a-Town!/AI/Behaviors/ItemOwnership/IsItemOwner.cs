using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class IsItemOwner : BehaviorCondition
    {
        readonly GearType Type;
        public IsItemOwner(GearType geartype)
        {
            this.Type = geartype;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //var target = state[this.TargetKey] as TargetArgs;
            //return OwnershipComponent.Owns(parent, target.Object) ? BehaviorState.Success : BehaviorState.Fail;

            var item = GearComponent.GetSlot(parent, this.Type);
            var owns = OwnershipComponent.Owns(parent, item.Object);
            return owns ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new IsItemOwner(this.Type);
        }

        //string TargetKey;
        //public IsItemOwner(string targetKey)
        //{
        //    this.TargetKey = targetKey;
        //}
        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    var target = state[this.TargetKey] as TargetArgs;
        //    return OwnershipComponent.Owns(parent, target.Object) ? BehaviorState.Success : BehaviorState.Fail;
        //}
        //public override object Clone()
        //{
        //    return new IsItemOwner(this.TargetKey);
        //}
    }
}
