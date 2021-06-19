using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorDrop : Behavior
    {
 
        //Func<GameObject, bool> Condition;
       
        string TargetName;
        string QuantityName;
        Interaction Goal;

        public BehaviorDrop(string target, string quantity)
        {
            this.TargetName = target;
            this.QuantityName = quantity;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (parent.Velocity != Vector3.Zero)
            {
                var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    parent.MoveToggle(false);
                return BehaviorState.Running;
            }
            var net = parent.Net;

            // DONT DO THAT HERE BECAUSE SUCCESSFULLY FINISHING THE INTERACTION MEANS ITS CANCELSTATE IS TRUE!!!
            //if(job.IsCancelled(parent))
            //{
            //    state.StopJob();
            //    (net as Server).AIHandler.AIInterrupt(parent);
            //}

            //var args = state.Blackboard[this.ArgsName] as PickUpArgs;
            //TargetArgs target = args.Target;
            //Interaction goal = new PickUp(args.Quantity);
            var target = state.Blackboard[this.TargetName] as TargetArgs;
            var quantity = (int)state.Blackboard[this.QuantityName];
            //Interaction goal = new UseHauledOnTarget(quantity);
            if (this.Goal == null)
                this.Goal = new UseHauledOnTarget(quantity);

            switch (this.Goal.State)
            {
                case Interaction.States.Unstarted:
                    if (!this.Goal.IsValid(parent, target))
                    {
                        // arrived to perform interaction but interaction no longer valid!
                        // TODO: post message to ai log?
                        return BehaviorState.Fail;
                    }
                    (net as Server).AIHandler.AIInteract(parent, this.Goal, target);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    this.Goal = null;
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }

        public override object Clone()
        {
            return new BehaviorDrop(this.TargetName, this.QuantityName);// this.ArgsName);
        }
    }
}
