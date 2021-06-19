using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorEnsureDistance : Behavior
    {
        float MinRange;
        string TargetKey;
        public BehaviorEnsureDistance(string targetKey, float minRange)
        {
            this.TargetKey = targetKey;
            this.MinRange = minRange;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var target = state.GetBlackboardValue<TargetArgs>(this.TargetKey);
            var d = Vector3.Distance(parent.Global, target.Global);
            if (d > this.MinRange)
            {
                //if (parent.Acceleration > 0)
                //{
                //    AIManager.AIStopMove(parent);
                //    return BehaviorState.Running;
                //}
                //else
                //{
                //    if (parent.Velocity == Vector3.Zero)
                        return BehaviorState.Success;
                //}
            }
            if (parent.Acceleration == 0)
            {
                var dir = parent.Global - target.Global;
                dir.Normalize();
                parent.Direction = dir;
                AIManager.AIStartMove(parent);
            }
            return BehaviorState.Running;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
