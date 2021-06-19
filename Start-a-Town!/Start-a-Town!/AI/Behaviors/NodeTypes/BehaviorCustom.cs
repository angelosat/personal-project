using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class BehaviorCustom : Behavior
    {
        public enum Modes { Instant, Continuous }
        //Func<BehaviorState> Action;
        public Modes Mode = Modes.Instant;
        bool Initialized;
        public Action<Actor, AIState> Init = (parent, state) => { };
        public Action InitAction = () => { };
        public Action Tick = () => { };
        public Func<Actor, bool> FailCondition = (a) => false;
        public Func<Actor, bool> SuccessCondition = (a) => false;

        public BehaviorCustom()
        {

        }
        public BehaviorCustom(Action initAction)
        {
            this.InitAction = initAction;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if(!this.Initialized)
            {
                this.Actor = parent;
                this.Init(parent, state);
                this.InitAction();
                this.Initialized = true;
            }
            //this.Tick(); // MOVED THIS here because the behaviorhaulhelper.startcarrying behavior failed when the target item to be hauled was absorbed by the currently carried one
            // the behavior fails if the target is disposed and the fail condition was checked at the beginning of behaviorperformtask update function, before 
            // the task target was replaced by the currently carried item

            if (this.SuccessCondition(parent)) // MOVED THIS before failcondition after grabtool behavior wasn't working properly
                                               // MADE A CHANGE to grabtool behavior that made the move unnecessary
            {
                this.Initialized = false;
                return BehaviorState.Success;
            }
            if (this.FailCondition(parent))
            {
                this.Initialized = false;
                var actor = this.Actor;
                //this.Actor.NetNew.SyncReport($"{actor.CurrentTaskBehavior} failed at {this} ({this.Label})");
                return BehaviorState.Fail;
            }
            //if (this.SuccessCondition(parent))
            //{
            //    this.Initialized = false;
            //    return BehaviorState.Success;
            //}
            this.Tick();
            //return this.Mode == Modes.Instant ? BehaviorState.Success : BehaviorState.Running;
            if (this.Mode== Modes.Instant)
            {
                this.Initialized = false; 
                return BehaviorState.Success;
            }
            return BehaviorState.Running;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        //static public BehaviorCustom DropEquipped(GearType geartype)
        //{
        //    return new BehaviorCustom()
        //    {
        //        act
        //    }
        //}
    }
}
