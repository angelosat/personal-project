using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.PathFinding;
namespace Start_a_Town_.AI.Behaviors
{
    class PathExists : BehaviorCondition
    {
        public override bool Evaluate(GameObject agent, AIState state)
        {
            if (state.Path != null)
                return state.Path.Stack.Count > 0;
                //if (state.Path.Stack.Count == 0)
                //    throw new Exception();
            return false;
        }
        //string PathName;
        //public PathExists(string pathName)
        //{
        //    this.PathName = pathName;
        //}
        //public override bool Evaluate(GameObject agent, AIState state)
        //{
        //    object variable;
        //    if (state.Blackboard.TryGetValue(this.PathName, out variable))
        //    {
        //        return true;
        //        var path = variable as Path;
        //        if (path.Stack.Count > 0)
        //            return true;
        //    }
        //    return false;
        //}
    }
}
