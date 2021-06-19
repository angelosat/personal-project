using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.PathFinding;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFollowPathNewNew : Behavior
    {
        //bool Moving;
        //TargetArgs Target;
        float Range;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="range">Stop moving when within this range of the final path step</param>
        public BehaviorFollowPathNewNew(bool urgent = true, float range = .1f)
        {
            this.Range = range;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            state.Leash = parent.Global;
            var path = state.Path;

            // check if path invalidated
            if (path == null)
                return BehaviorState.Fail;

            //var next = state.Path.Stack.Peek();
            var next = path.Current;


            // todo: also check if inbetween blocks have changed? keep a list of blocks that the actor will walk upon when generating the path, and check them when handling blockchanged events?
            // todo: reserve path steps???
            if (!PathingSync.IsPathable(parent.Map, next))
                return BehaviorState.Fail;

            /// i check if path valid inside aicomponent by handling blockchanged event
            //if (!path.IsValid(parent))
            //    return BehaviorState.Fail;

            var acc = parent.Acceleration;

            if (acc > 0) //if moving
            {
                //if (IsLastStep(state) ? parent.IsAt(next) : parent.IsFootprintWithinBlock(next))// IsAtDestination(parent, next))
                if (parent.IsFootprintWithinBlock(next))// IsAtDestination(parent, next))
                {
                    if (IsLastStep(state)) // is last step, so make sure we are at the destination and not falling, before popping path step and stopping movement
                    {
                        parent.MoveToggle(false);
                        return BehaviorState.Running;
                    }
                    else
                        //path.Stack.Pop();
                        path.MoveNext();
                }
            }
            else // if not moving, and not at goal, start moving
            {
                if (parent.IsFootprintWithinBlock(next))// IsAtDestination(parent, next))
                {
                    if (IsLastStep(state))
                    {
                        if (parent.Velocity == Vector3.Zero)
                        {
                            state.Path = null;
                            return BehaviorState.Success;
                        }
                        else
                            return BehaviorState.Running;
                    }
                }
                parent.MoveToggle(true);
                //parent.WalkToggle(false);
                parent.WalkToggle(!parent.CurrentTask.Urgent);

                //(parent.Net as Server).AIHandler.AIStartMove(parent);
                //(parent.Net as Server).AIHandler.AIToggleWalk(parent, false);
            }
            Vector3 distanceVector = next - parent.Global;
            Vector2 directionNormal = distanceVector.XY();
            directionNormal.Normalize();
            parent.Direction = new Vector3(directionNormal, 0);
            return BehaviorState.Running;
        }
        //public BehaviorState ExecuteOld(Actor parent, AIState state)
        //{
        //    state.Leash = parent.Global;
        //    var path = state.Path;
        //    if (!path.Stack.Any())
        //    {
        //        // run until fully stopped
        //        if (parent.Velocity == Vector3.Zero)
        //            return BehaviorState.Success;
        //        else
        //            return BehaviorState.Running;
        //    }
        //    var next = state.Path.Stack.Peek();
        //    // todo: also check if inbetween blocks have changed? keep a list of blocks that the actor will walk upon when generating the path, and check them when handling blockchanged events?
        //    // todo: reserve path steps???
        //    if (!PathingSync.IsPathable(parent.Map, next))
        //        return BehaviorState.Fail;
        //    var acc = parent.Acceleration;

        //    if (acc > 0) //if moving
        //    {
        //        //if (IsLastStep(state) ? parent.IsAt(next) : parent.IsFootprintWithinBlock(next))// IsAtDestination(parent, next))
        //        if (parent.IsFootprintWithinBlock(next))// IsAtDestination(parent, next))
        //        {
        //            if (IsLastStep(state)) // is last step, so make sure we are at the destination and not falling, before popping path step and stopping movement
        //            {
        //                if (parent.Velocity.Z == 0)
        //                {
        //                    path.Stack.Pop();
        //                    parent.MoveToggle(false);
        //                    return BehaviorState.Running;
        //                }
        //            }
        //            else
        //                path.Stack.Pop();
        //        }

        //        //if (IsAtDestination(parent, next, .1f))
        //        //{
        //        //    if (IsLastStep(state)) // is last step, so make sure we are at the destination and not falling, before popping path step and stopping movement
        //        //    {
        //        //        if (parent.Velocity.Z == 0)
        //        //        {
        //        //            path.Stack.Pop();
        //        //            AIManager.AIStopMoveNew(parent);
        //        //            return BehaviorState.Running;
        //        //        }
        //        //    }
        //        //    else
        //        //        path.Stack.Pop();
        //        //}
        //    }
        //    else // if not moving, start moving
        //    {
        //        parent.MoveToggle(true);
        //        parent.WalkToggle(false);
        //    }
        //    Vector3 distanceVector = next - parent.Global;
        //    Vector2 directionNormal = distanceVector.XY();
        //    directionNormal.Normalize();
        //    parent.Direction = new Vector3(directionNormal, 0);
        //    return BehaviorState.Running;
        //}

        private static bool IsLastStep(AIState state)
        {
            return state.Path.IsLastStep;
            //return state.Path.Stack.Count == 1;
        }
        
        
        public override object Clone()
        {
            throw new Exception();
        }
    }

    
}
