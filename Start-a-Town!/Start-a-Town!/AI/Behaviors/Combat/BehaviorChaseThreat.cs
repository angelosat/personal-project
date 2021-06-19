using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorChaseThreat : Behavior
    {
        float Range;
        /// <summary>
        /// TODO: compute range from equipped weapon
        /// </summary>
        /// <param name="range"></param>
        public BehaviorChaseThreat(float range = Attack.DefaultRange)
        {
            this.Range = range;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var threat = state.Threats.FirstOrDefault();
            var entity = threat.Entity;
            var diff = entity.Global - parent.Global;
            var dist = diff.Length();
            if (dist <= this.Range)// 1)
                AIManager.AIStopMove(parent);
            else
            {
                var dir = diff.XY();
                dir.Normalize();
                //parent.Direction = new Vector3(dir.X, dir.Y, 0);
                AIManager.AIMove(parent, new Vector3(dir.X, dir.Y, 0));
            }
            // TODO: stop fleeing after certain amount of range. here? or separate behavior?
            return BehaviorState.Success;
            //throw new NotImplementedException();
        }
        public override object Clone()
        {
            return new BehaviorChaseThreat(this.Range);
        }
    }
}
