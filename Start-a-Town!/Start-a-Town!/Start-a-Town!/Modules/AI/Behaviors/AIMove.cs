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
    class AIMove : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIMove";
            }
        }

        bool Moving;
        //float RangeMin, RangeMax;

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            //var move = state.MoveTarget;
            //if (move == null)
            //    return BehaviorState.Fail;
            //Vector3 distanceVector = move.Target.Global - parent.Global;
            //var l = distanceVector.Length();
            //if (move.RangeMin <= l && l <= move.RangeMax)
            //{
            //    (net as Server).AIHandler.AIStopMove(parent);
            //    this.Moving = false;
            //    return BehaviorState.Fail;
            //}
            //Vector2 directionNormal = distanceVector.XY();
            //directionNormal.Normalize();
            //if (l < move.RangeMin)
            //    directionNormal *= -1;
            //(net as Server).AIHandler.AIChangeDirection(parent, new Vector3(directionNormal, 0));
            //if (!this.Moving)
            //{
            //    (net as Server).AIHandler.AIStartMove(parent);
            //    (net as Server).AIHandler.AIToggleWalk(parent, false);
            //    this.Moving = true;
            //}
            //return BehaviorState.Success;
            var net = parent.Net;
            AIJob job = state.GetJob();// state.Job;

            if (job == null)
                return BehaviorState.Fail;
            AIInstruction step = job.Instructions.First();
            TargetArgs target = step.Target;
            Interaction goal = step.Interaction;

            Vector3 distanceVector = target.Global - parent.Global;
            if (distanceVector.Length() <= InteractionOld.DefaultRange)
            {
                //parent.TryGetComponent<MobileComponent>(c => c.Stop(parent));
                (net as Server).AIHandler.AIStopMove(parent);
                this.Moving = false;
                return BehaviorState.Success;
            }
            Vector2 directionNormal = distanceVector.XY();
            directionNormal.Normalize();
            (net as Server).AIHandler.AIChangeDirection(parent, new Vector3(directionNormal, 0));
            //actor.Transform.Direction = directionNormal;
            if (!this.Moving)
            {
                //parent.TryGetComponent<MobileComponent>(c => c.Start(parent));
                (net as Server).AIHandler.AIStartMove(parent);
                (net as Server).AIHandler.AIToggleWalk(parent, false);
                this.Moving = true;
            }
            return BehaviorState.Running;
        }

        public override object Clone()
        {
            return new AIMove();
        }
    }
}
