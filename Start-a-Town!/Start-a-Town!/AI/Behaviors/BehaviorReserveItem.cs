using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorReserveItem : Behavior
    {
        string TargetKey;
        int Amount;
        public BehaviorReserveItem(string targetKey, int amount)
        {
            this.TargetKey = targetKey;
            this.Amount = amount;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var target = state[this.TargetKey] as TargetArgs;
            state.Reserve(parent, target.Object, this.Amount);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            //throw new NotImplementedException();
            return new BehaviorReserveItem(this.TargetKey, this.Amount);
        }
    }
}
