using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindPathStack : Behavior
    {
        string TargetName, PathName = "Path";
        PathingSync Pathing = new PathingSync();
        TargetArgs Target;
        float Range;
        public BehaviorFindPathStack(string variableName, string pathName, float range = 0)
        {
            this.TargetName = variableName;
            this.Range = range;
            this.PathName = pathName;
            //this.Pathing = new PathingSync();
        }
        public BehaviorFindPathStack(string variableName, float range = 0)
        {
            this.TargetName = variableName;
            this.Range = range;
            //this.Pathing = new PathingSync();
        }
        public BehaviorFindPathStack(TargetArgs target, float range = 0)
        {
            this.Target = target;
            this.Range = range;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.Target == null)
                this.Target = state.Blackboard[this.TargetName] as TargetArgs;

            switch(this.Pathing.State)
            {
                case PathingSync.States.Stopped:
                    this.Pathing.Begin(parent, parent.Global, this.Target.Global, this.Range);
                    this.Pathing.Work();
                    return BehaviorState.Running;

                case PathingSync.States.Working:
                    this.Pathing.Work();
                    return BehaviorState.Running;

                case PathingSync.States.Finished:
                    this.Target = null;
                    var path = this.Pathing.GetPath();
                    state.Blackboard[this.PathName] = path.Stack;
                    return BehaviorState.Success;

                default:
                    break;
            }
            throw new NotImplementedException();
            //return base.Execute(parent, state);
        }

        public override object Clone()
        {
            return new BehaviorFindPathStack(this.TargetName);
        }
    }
}
