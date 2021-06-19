using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorCreatePath : Behavior
    {
        string PathKey;
        public BehaviorCreatePath(string pathKey)
        {
            this.PathKey = pathKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            state[this.PathKey] = new Stack<Vector3>();
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorCreatePath(this.PathKey);
        }
    }
}
