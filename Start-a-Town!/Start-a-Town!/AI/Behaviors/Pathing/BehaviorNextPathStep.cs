using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorNextPathStep : Behavior
    {
        string PathKey, TargetKey;
        public BehaviorNextPathStep(string pathKey, string targetKey)
        {
            this.PathKey = pathKey;
            this.TargetKey = targetKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (!state.Blackboard.ContainsKey(this.PathKey))
                return BehaviorState.Fail;
            var path = state[this.PathKey] as Stack<Vector3>;
            if(!path.Any())
                return BehaviorState.Fail;
            var next = path.Pop();
            state[this.TargetKey] = new TargetArgs(parent.Map, next);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorNextPathStep(this.PathKey, this.TargetKey);
        }
    }
}
