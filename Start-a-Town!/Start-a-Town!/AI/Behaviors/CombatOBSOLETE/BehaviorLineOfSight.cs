using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorLineOfSight : Behavior
    {
        string LastKnownPositionKey;
        public BehaviorLineOfSight(string lastKnownPositionKey)
        {
            this.LastKnownPositionKey = lastKnownPositionKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var threat = state.Threats.First().Entity;
            //var los = parent.Map.LineOfSight(parent.Global, threat.Global);
            var a = parent.Global.Round();
            var b = threat.Global.Round();
            var x0 = (int)a.X;
            var y0 = (int)a.Y;
            var z0 = (int)a.Z;
            var x1 = (int)b.X;
            var y1 = (int)b.Y;
            var z1 = (int)b.Z;
            var los = LineHelper.LineOfSight(x0, y0, z0, x1, y1, z1, threat.Map.IsSolid);
            if (los)
            {
                state[this.LastKnownPositionKey] = new TargetArgs(threat.Map, b);// threat.Global);
                return BehaviorState.Success;
            }
            else
                return BehaviorState.Fail;
            //return los ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorLineOfSight(this.LastKnownPositionKey);
        }
    }
}
