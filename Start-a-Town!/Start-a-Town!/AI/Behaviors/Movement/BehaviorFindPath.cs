using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindPath : Behavior
    {
        string TargetName, PathName= "path";
        int TargetInd;
        //PathingSync Pathing = new PathingSync();
        //TargetArgs Target { get { return this.Actor.CurrentTask.Targets[this.TargetInd]; } set { } }
        TargetArgs Target { get { return this.Actor.CurrentTask.GetTarget(this.TargetInd); } set { } }

        float Range;
        PathingSync.FinishMode FinishMode = PathingSync.FinishMode.Touching;
        public BehaviorFindPath(string variableName, string pathName, float range = 0)
        {
            this.TargetName = variableName;
            this.Range = range;
            this.PathName = pathName;
        }
        public BehaviorFindPath(string variableName, float range = 0)
        {
            this.TargetName = variableName;
            this.Range = range;
        }
        
        public BehaviorFindPath(TargetArgs target, float range = 0)
        {
            this.Target = target;
            this.Range = range;
        }

        public BehaviorFindPath(TargetArgs target, string pathname)
            : this(target, PathingSync.FinishMode.Touching, pathname)
        {

        }
        public BehaviorFindPath(TargetArgs target, PathingSync.FinishMode mode, string pathname)
        {
            this.Target = target;
            this.PathName = pathname;
            this.FinishMode = mode;
        }
        public BehaviorFindPath(int targetInd, PathingSync.FinishMode mode, string pathname)
        {
            this.TargetInd = targetInd;
            this.PathName = pathname;
            this.FinishMode = mode;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            // only execute if there's no current path
            if (state.Path != null)
                return BehaviorState.Success;

            this.Actor = parent;

            switch (state.PathFinder.State)
            {
                case PathingSync.States.Stopped:
                    state.PathFinder.Begin(parent, parent.StandingOn().Above(), this.Target.Global, this.Range);
                    state.PathFinder.WorkMode(this.FinishMode);
                    return BehaviorState.Running;

                case PathingSync.States.Working:
                    //state.PathFinder.WorkModeStep(this.FinishMode);
                    state.PathFinder.WorkMode(this.FinishMode);
                    return BehaviorState.Running;

                case PathingSync.States.Finished:
                    //this.Target = null;

                    var path = state.PathFinder.GetPath();
                    state.Blackboard[this.PathName] = path;
                    state.Path = path;
                    //if (path == null)
                    //    throw new Exception();
                    //this.Target = null;

                    //parent.MoveToggle(true);
                    //parent.WalkToggle(!state.CurrentTask.Urgent);
                    ////parent.WalkToggle(false);

                    //return path != null ? BehaviorState.Success : BehaviorState.Fail;
                    if (path is not null)
                    {
                        parent.MoveToggle(true);
                        parent.WalkToggle(!state.CurrentTask.Urgent);
                        return BehaviorState.Success;
                    }
                    else
                    {
                        parent.Net.SyncReport($"{parent.Name} failed to find path from {parent.StandingOn().Above()} to {this.Target.Global}! {state.PathFinder.Ticks}");
                        return BehaviorState.Fail;
                    }


                default:
                    break;
            }
            throw new Exception();
        }

        public override object Clone()
        {
            return new BehaviorFindPath(this.TargetName, this.PathName, this.Range);
        }
    }
}
