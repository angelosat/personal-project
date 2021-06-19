using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI
{
    class AIMoveTo : Behavior
    {
        //enum States { FollowingPath, Chasing }
        public override string Name
        {
            get
            {
                return "AIMoveTo";
            }
        }
        public override object Clone()
        {
            return new AIMoveTo();
        }

        static int CheckLosTimerMax = Engine.TargetFps;
        int CheckLosTimer = CheckLosTimerMax;
        bool Moving;
        //float RangeMin, RangeMax;
        
        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            if(state.Pathfinder.State == PathFinding.PathingSync.States.Working)
            {
                state.Pathfinder.Work();
                return BehaviorState.Running;
            }
            else if(state.Pathfinder.State == PathFinding.PathingSync.States.Finished)
            {
                state.SetPath(state.Pathfinder.GetPath());
                "path recalculated: ".ToConsole();
                state.Path.ToString();
            }
            var net = parent.Net;
            var server = net as Server;
            // TODO: decouple job from moveto behavior
            //CheckInteraction(parent, state);
            AIJob job = state.GetJob();// state.Job;
            if (job == null)
            {
                server.AIHandler.AIStopMove(parent);
                server.AIHandler.AIThrow(parent, Vector3.Zero, true);
                this.Moving = false;
                return BehaviorState.Fail;
            }

            AIInstruction step = job.CurrentStep;
            TargetArgs target = step.Target;
            Interaction goal = step.Interaction;
            if (goal.IsCancelled(parent, target))
            {
                // arrived to perform interaction but interaction no longer valid!
                // TODO: post message to ai log?
                state.StopJob();
                server.AIHandler.AIStopMove(parent);
                server.AIHandler.AIThrow(parent, Vector3.Zero, true);
                return BehaviorState.Fail;
            }

            state.Leash = parent.Global;
            var move = state.MoveTarget;
            Vector3 distanceVector = move.Target.Global - parent.Global;
            var l = distanceVector.Length();
            var dist = Vector3.Distance(move.Target.Global, parent.Global);
            if (this.Moving)
            {
                if (move.RangeMin <= l && l <= move.RangeMax)
                {
                    //destination reached
                    if (state.Path != null)
                    {
                        if (state.Path.Stack.Count > 0)
                        {
                            float rmin = 0f, rmax = .1f;
                            //var job = state.GetJob();
                            if (job != null)
                            {
                                //var goal = job.CurrentStep;
                                var range = goal.RangeCheckCached;//.Conditions.GetAllChildren().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
                                if (range != null)
                                {
                                    rmin = range.Min;
                                    rmax = range.Max;
                                }
                            }

                            if (state.Path.Stack.Count > 1)
                            {
                                rmin = 0;
                                rmax = .1f;
                            }

                            // if we're at the last step of the path, replace last step with the target of the current interaction
                            //var nextTarget = state.Path.Stack.Count == 1 ? job.CurrentStep.Target : new TargetArgs(state.Path.Stack.Pop());
                            TargetArgs nextTarget;
                            if (state.Path.Stack.Count == 1)
                            {
                                nextTarget = job.CurrentStep.Target;
                                state.Path.Stack.Pop();
                            }
                            else
                            {
                                // get height of block below and adjust target Z accordingly as to stop the agent gyrating when block below the target position is low height causing it to be out of range
                                // TODO: do this when building the path or when setting path state, because when i do it here the first step isn't handleded
                                // FIX IT
                                var pos = state.Path.Stack.Pop();
                                //var blockHeight = Block.GetBlockHeight(parent.Map, pos - Vector3.UnitZ);
                                //pos.Z -= (1 - blockHeight);
                                nextTarget = new TargetArgs(pos);
                            }
                            state.MoveTarget = new AITarget(nextTarget, rmin, rmax);// 0, 0.1f);
                            //state.MoveTarget = new AITarget(new TargetArgs(state.Path.Stack.Pop()), rmin, rmax);// 0, 0.1f);
                            return BehaviorState.Running;
                        }
                    }
                    (net as Server).AIHandler.AIStopMove(parent);
                    this.Moving = false;
                    return BehaviorState.Success;
                }
                else
                {
                    // if moving but destination not reached
                    if (!CheckLos(parent, state))
                    {
                        "los lost".ToConsole();
                        // if lost line of sight, stop moving and start calculating a new path
                        PathTo(state, parent.Map, parent.Global, step.Target.Global);// state.MoveTarget.Target.Global);                        
                        return BehaviorState.Running;
                    }
                }
            }
            else
            {
                if (move.RangeMin <= l && l <= move.RangeThreshold)
                    return BehaviorState.Success;
            }   

            // if not moving

            Vector2 directionNormal = distanceVector.XY();
            directionNormal.Normalize();

            //move away to maintain a minimum distance
            if (l < move.RangeMin)
                directionNormal *= -1;  

            //(net as Server).AIHandler.AIChangeDirection(parent, new Vector3(directionNormal, 0)); // not needed? changing direction at clients anyway when sending snapshots?
            parent.Direction = new Vector3(directionNormal, 0);
            if (!this.Moving)
            {
                (net as Server).AIHandler.AIStartMove(parent);
                (net as Server).AIHandler.AIToggleWalk(parent, false);
                this.Moving = true;
            }
            return BehaviorState.Running;

            //if (state.Job == null)
            //    return BehaviorState.Fail;
            //AIJob job = state.Job;
            //AIInstruction step = state.Job.Instructions.First();
            //TargetArgs target = step.Target;
            //Interaction goal = step.Interaction;

            //Vector3 distanceVector = target.Global - parent.Global;
            //if (distanceVector.Length() <= InteractionOld.DefaultRange)
            //{
            //    //parent.TryGetComponent<MobileComponent>(c => c.Stop(parent));
            //    (net as Server).AIHandler.AIStopMove(parent);
            //    this.Moving = false;
            //    return BehaviorState.Success;
            //}
            //Vector2 directionNormal = distanceVector.XY();
            //directionNormal.Normalize();
            //(net as Server).AIHandler.AIChangeDirection(parent, new Vector3(directionNormal, 0));
            ////actor.Transform.Direction = directionNormal;
            //if (!this.Moving)
            //{
            //    //parent.TryGetComponent<MobileComponent>(c => c.Start(parent));
            //    (net as Server).AIHandler.AIStartMove(parent);
            //    (net as Server).AIHandler.AIToggleWalk(parent, false);
            //    this.Moving = true;
            //}
            //return BehaviorState.Running;
        }

        private void StopMoving(GameObject parent)
        {
            this.Moving = false;
            (parent.Net as Server).AIHandler.AIStopMove(parent);
        }

        bool CheckLos(GameObject agent, AIState state)
        {
            if(CheckLosTimer > 0)
            {
                CheckLosTimer--;
                return CheckLosNextStep(agent, state);
            }
            // WARNING: can miss checking next step?
            CheckLosTimer = CheckLosTimerMax;
            return CheckLosFull(agent, state);

            //if (!PathFinding.PathingSync.IsWalkable(map, nextStep))
            //{
            //    // find and assign new path
            //    PathTo(state, map, current, goal);
            //    return false;
            //}
            //return true;
        }

        private bool CheckLosFull(GameObject agent, AIState state)
        {
            return PathFinding.PathingSync.LineOfSight(agent.Map, agent.Global.Round(), state.MoveTarget.Target.Global.Round());
        }

        private static bool CheckLosNextStep(GameObject agent, AIState state)
        {
            var t = state.MoveTarget.Target;
            if (t.Type == TargetType.Position) // temporary 
                return true; // TODO: remove this restriction
            var map = agent.Map;
            var goal = t.Global;
            var current = agent.Global;
            var d = agent.Direction;
            var nextStep = current + d; // do i have to account friction?
            //return PathFinding.PathingSync.IsWalkable(map, nextStep);
            if (!PathFinding.PathingSync.IsWalkable(map, nextStep))
                if (!PathFinding.PathingSync.IsWalkable(map, nextStep + Vector3.UnitZ)) // entity can neither climb on block 
                    return false;
            return true;
        }

        static bool InstantPathRecalculate = true;
        private void PathTo(AIState state, GameModes.IMap map, Vector3 current, Vector3 goal)
        {
            state.Pathfinder.Begin(map, current, goal);
            if (!InstantPathRecalculate)
            {
                StopMoving(state.Parent);
                return;
            }
            while (state.Pathfinder.State != PathFinding.PathingSync.States.Finished)
                state.Pathfinder.Work();
            state.SetPath(state.Pathfinder.GetPath());
        }

        static void CheckInteraction(GameObject parent, AIState state)
        {
            var job = state.GetJob();
            if (job == null)
                return;
            if(job.Source.IsCancelled(parent))
                state.StopJob();
            var i = job.CurrentStep;
            if (i.Interaction.IsCancelled(parent, i.Target))
                state.StopJob();
        }   
    }
}
