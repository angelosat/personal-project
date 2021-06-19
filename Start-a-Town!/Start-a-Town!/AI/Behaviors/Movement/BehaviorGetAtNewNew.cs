using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorGetAtNewNew : BehaviorQueue
    {
        public BehaviorGetAtNewNew(TargetIndex targetInd)//, int range = 1, bool urgent = true)
            : this((int)targetInd, PathingSync.FinishMode.Touching)//, range, urgent)
        {

        }
        public BehaviorGetAtNewNew(int targetInd)//, int range = 1, bool urgent = true)
            : this(targetInd, PathingSync.FinishMode.Touching)//, range, urgent)
        {

        }
        public BehaviorGetAtNewNew(TargetArgs target)//, int range = 1, bool urgent = true)
            : this(target, PathingSync.FinishMode.Touching)//, range, urgent)
        {

        }
        public BehaviorGetAtNewNew(TargetArgs target, PathingSync.FinishMode mode)//, int range = 1, bool urgent = true)
        {
            this.Children = new List<Behavior>(){
                    new BehaviorOpenDoor(),
                    new BehaviorInverter(new BehaviorJumpOnBlock()),
                    new BehaviorInverter(new BehaviorCrouch()),
                    new BehaviorInverter(new BehaviorUnstuck()),
                    new BehaviorQueue(
                        new BehaviorInverter(new BehaviorFindPath(target, mode, "path")), // TODO: completely fail behavior if no path found
                        //new BehaviorStartMoving(urgent),
                        new BehaviorFollowPathNewNew()) // TODO: if path is invalidated while following, return to the find path behavior to find a new path
            };

        }
        public BehaviorGetAtNewNew(TargetIndex targetInd, PathingSync.FinishMode mode)//, int range = 1, bool urgent = true)
            :this((int)targetInd, mode)//, range, urgent)
        {

        }
        public BehaviorGetAtNewNew(int targetInd, PathingSync.FinishMode mode)//, int range = 1, bool urgent = true)
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
        public BehaviorGetAtNewNew(string target)//, float maxrange)//, float minrange = .1f)
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
