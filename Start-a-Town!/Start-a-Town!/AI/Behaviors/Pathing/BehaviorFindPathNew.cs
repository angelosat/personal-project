using System;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindPathNew : Behavior
    {
        string TargetKey, PathKey;
        PathingSync Pathing = new PathingSync();
        TargetArgs Target;
        float Range;
        public BehaviorFindPathNew(string variableName, string pathName, float range = 0)
        {
            this.TargetKey = variableName;
            this.Range = range;
            this.PathKey = pathName;
        }
        public BehaviorFindPathNew(string variableName, float range = 0)
        {
            this.TargetKey = variableName;
            this.Range = range;
        }
        public BehaviorFindPathNew(TargetArgs target, float range = 0)
        {
            this.Target = target;
            this.Range = range;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.Target == null)
                this.Target = state.Blackboard[this.TargetKey] as TargetArgs;

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
                    state.Blackboard[this.PathKey] = path.Stack;
                    return BehaviorState.Success;

                default:
                    break;
            }
            throw new Exception();
        }

        public override object Clone()
        {
            return new BehaviorFindPathNew(this.TargetKey, this.PathKey);
        }
    }
}
