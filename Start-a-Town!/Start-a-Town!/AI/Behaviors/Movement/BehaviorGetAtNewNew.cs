using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorGetAtNewNew : BehaviorQueue
    {
        public BehaviorGetAtNewNew(TargetIndex targetInd)
            : this((int)targetInd, PathingSync.FinishMode.Touching)
        {

        }
        public BehaviorGetAtNewNew(TargetArgs target)
            : this(target, PathingSync.FinishMode.Touching)
        {

        }
        public BehaviorGetAtNewNew(TargetArgs target, PathingSync.FinishMode mode)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorOpenDoor(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(target, mode, "path")), // TODO: completely fail behavior if no path found
                        new BehaviorFollowPathNewNew()) // TODO: if path is invalidated while following, return to the find path behavior to find a new path
            };
        }
        public BehaviorGetAtNewNew(TargetIndex targetInd, PathingSync.FinishMode mode)
            :this((int)targetInd, mode)
        {

        }
        public BehaviorGetAtNewNew(int targetInd, PathingSync.FinishMode mode)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorOpenDoor(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(targetInd, mode, "path")),
                        new BehaviorFollowPathNewNew())
            };
        }
        public BehaviorGetAtNewNew(string target)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorSequence(
                        new BehaviorFindPath(target, "path"),
                        new BehaviorSetPath("path"),
                        new BehaviorStartMoving(),
                        new BehaviorFollowPathNewNew())
            };
        }
        
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
    }
}
