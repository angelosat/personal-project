using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorThreatsExist : Behavior
    {
        string LastKnownPositionKey;
        public BehaviorThreatsExist(string lastKnownPositionKey)
        {
            this.LastKnownPositionKey = lastKnownPositionKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //return state.Threats.Count > 0 ? BehaviorState.Success : BehaviorState.Fail;
            var threat = state.Threats.FirstOrDefault();
            if(threat!=null)
            {
                state[this.LastKnownPositionKey] = new TargetArgs(threat.Entity);// new TargetArgs(parent.Map, threat.Entity.Global);
                return BehaviorState.Success;
            }
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorThreatsExist(this.LastKnownPositionKey);
        }
    }
}
