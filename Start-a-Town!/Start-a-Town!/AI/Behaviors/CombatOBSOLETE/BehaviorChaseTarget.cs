using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorChaseTarget : Behavior
    {
        float Range;
        string TargetKey;
        /// <summary>
        /// TODO: compute range from equipped weapon
        /// </summary>
        /// <param name="range"></param>
        public BehaviorChaseTarget(string targetKey, float range = Attack.DefaultRange)
        {
            this.TargetKey = targetKey;
            this.Range = range;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (!state.Blackboard.ContainsKey(this.TargetKey))
                return BehaviorState.Fail;
            var target = state[this.TargetKey] as TargetArgs;
            var targetPos = target.Global;
            var diff = targetPos - parent.Global;
            var dist = diff.Length();
            if (dist <= this.Range)// 1)
            {
                AIManager.AIStopMove(parent);
                return BehaviorState.Success;

            }
            else
            {
                var dir = diff.XY();
                dir.Normalize();
                //parent.Direction = new Vector3(dir.X, dir.Y, 0);
                (parent.Net as Server).AIHandler.AIToggleWalk(parent, false);
                AIManager.AIMove(parent, new Vector3(dir.X, dir.Y, 0));
                return BehaviorState.Running;

            }
            // TODO: stop fleeing after certain amount of range. here? or separate behavior?
            //return BehaviorState.Success;
            //throw new NotImplementedException();
        }
        public override object Clone()
        {
            return new BehaviorChaseTarget(this.TargetKey, this.Range);
        }
    }
}
