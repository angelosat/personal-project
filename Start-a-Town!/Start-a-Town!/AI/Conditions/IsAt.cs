using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI.Behaviors
{
    class IsAt : BehaviorCondition
    {
        TargetArgs Target;
        string VariableName;
        float Range;
        public IsAt(string variableName, float range)
        {
            this.VariableName = variableName;
            this.Range = range;
        }
        public IsAt(TargetArgs target, float range)
        {
            this.Target = target;
            this.Range = range;
        }
        //public override bool Evaluate(GameObject agent, AIState state)
        //{
        //    var target = this.Target ?? state.Blackboard[this.VariableName] as TargetArgs;
        //    switch (target.Type)
        //    {
        //        case TargetType.Entity:
        //            var cylindermax = new BoundingCylinder(target.Global, this.Range, 1);
        //            var result = cylindermax.Contains(agent.Global);
        //            return result;

        //        case TargetType.Position:
        //            var box = agent.GetBox();
        //            //var blockbox = target.Global.GetBox();
        //            var blockbox = new BoundingBox(target.Global - new Vector3(1, 1, 0), target.Global + new Vector3(1, 1, 1));

        //            return box.Intersects(blockbox);
        //            break;

        //        default:
        //            break;
        //    }
        //    throw new Exception();
        //}
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var target = this.Target ?? state.Blackboard[this.VariableName] as TargetArgs;
            var res = agent.IsInInteractionRange(target);
            return res;
            //var cylindermax = new BoundingCylinder(target.Global, RangeCheck.DefaultRange, 1);

            //var result = cylindermax.Contains(agent.Global);
            //return result;

            //var l = Vector3.Distance(target.Global, agent.Global);
            //return l <= this.Range;// .1f;
        }
    }
}
