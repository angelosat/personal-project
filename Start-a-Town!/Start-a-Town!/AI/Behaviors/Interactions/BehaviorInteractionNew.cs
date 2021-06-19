using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class BehaviorInteractionNew : Behavior
    {
        //Func<GameObject, bool> Condition;
        readonly string TargetVariableName;
        //readonly string InteractionKey;
        readonly int TargetInd;
        TargetArgs Target { get { return this.TargetGetter?.Invoke() ?? this.Actor.CurrentTask.GetTarget(this.TargetInd); } set { } }
        Interaction _Interaction;
        public Func<Interaction> InteractionFactory;
        readonly Func<TargetArgs> TargetGetter;
        Interaction Interaction
        {
            get
            {
                if (this._Interaction == null)
                    //    this._Interaction = this.Actor.GetComponent<WorkComponent>().Task;
                    this._Interaction = this.InteractionFactory();
                return this._Interaction;
            }
            set { this._Interaction = value; }
        }
        public BehaviorInteractionNew(TargetIndex targetInd, Interaction interaction) : this((int)targetInd, interaction)
        { }
        public BehaviorInteractionNew(TargetIndex targetInd, Func<Interaction> interactionFactory) : this((int)targetInd, interactionFactory)
        { }
        public BehaviorInteractionNew(Func<TargetArgs> targetGetter, Func<Interaction> interactionFactory) 
        {
            this.InteractionFactory = interactionFactory;
            this.TargetGetter = targetGetter;
        }
        public BehaviorInteractionNew(TargetIndex targetInd)
        {
            this.TargetInd = (int)targetInd;
        }

        public BehaviorInteractionNew(int targetInd, Func<Interaction> interactionFactory)
        {
            // TODO: Complete member initialization
            this.TargetInd = targetInd;
            this.InteractionFactory = interactionFactory;
        }
        public BehaviorInteractionNew(int targetInd, Interaction interaction)
        {
            // TODO: Complete member initialization
            this.TargetInd = targetInd;
            this.Interaction = interaction;
        }
        public BehaviorInteractionNew(TargetArgs targetArgs, Interaction interaction)
        {
            // TODO: Complete member initialization
            this.Target = targetArgs;
            this.Interaction = interaction;
        }
        public BehaviorInteractionNew(Interaction interaction)
        {
            this.Interaction = interaction;
        }
        public BehaviorInteractionNew(Func<Interaction> interactionFactory)
        {
            this.InteractionFactory = interactionFactory;
        }
        public BehaviorInteractionNew(string targetVariableName, Interaction interaction)
        {
            this.Interaction = interaction;
            this.TargetVariableName = targetVariableName;
        }
        //public BehaviorInteractionNew(string targetVariableName, string interactionKey)
        //{
        //    //this.InteractionKey = interactionKey;
        //    this.TargetVariableName = targetVariableName;
        //}


        public override BehaviorState Execute(Actor parent, AIState state)
        {
            this.Actor = parent;
            if (parent.Velocity != Vector3.Zero)
            {
                var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    parent.MoveToggle(false);
                return BehaviorState.Running;
            }
            var net = parent.Net;


            TargetArgs target = this.Target;// ?? (string.IsNullOrWhiteSpace(this.TargetVariableName) ? TargetArgs.Null : state.Blackboard[this.TargetVariableName] as TargetArgs);
            //Interaction goal = state.Blackboard.GetValueOrDefault("interaction") as Interaction;


            Interaction goal = this.Interaction;// ?? state.Blackboard[this.InteractionKey] as Interaction;
            //var currentInteraction = parent.GetComponent<WorkComponent>().Task;
            //Interaction goal = currentInteraction ?? (this.Interaction ?? state.Blackboard[this.InteractionKey] as Interaction);


            switch (goal.State)
            {
                case Interaction.States.Unstarted:
                    //if (!goal.IsValid(parent, target))
                    //{
                    //    // arrived to perform interaction but interaction no longer valid!
                    //    // TODO: post message to ai log?
                    //    return BehaviorState.Fail;
                    //}
                    (net as Server).AIHandler.AIInteract(parent, goal, target);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                   
                    this.Interaction = null; // THIS IS REQUIRED because when ths behavior is run again (in loops) it needs to create a new instance of the interaction
                    // WHY HAVE I COMMENTED IT OUT?
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    //"interaction failed".ToConsole();
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            // TODO: don't replace fresh stored interaction with null, if parent isn't currently interacting? very hacky
            this.Interaction = parent.GetComponent<WorkComponent>().Task ?? this.Interaction; 
        }
        public override object Clone()
        {
            //throw new Exception();
            return new BehaviorInteractionNew(this.TargetVariableName, this.Interaction.Clone() as Interaction);
        }
        //public override bool HasFailedOrEnded()
        //{
        //    if (this.Interaction.State == Interaction.States.Finished)
        //        return false;
        //    return base.HasFailedOrEnded();
        //}
    }

    //class BehaviorInteractionNewOld : Behavior
    //{
    //    Func<GameObject, bool> Condition;
    //    string TargetVariableName;
    //    Interaction Interaction;

    //    public BehaviorInteractionNewOld(string targetVariableName, Interaction interaction)
    //    {
    //        this.Interaction = interaction;
    //        this.TargetVariableName = targetVariableName;
    //    }

    //    public override BehaviorState Execute(Entity parent, AIState state)
    //    {
    //        if (parent.Velocity != Vector3.Zero)
    //        {
    //            var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
    //            if (acceleration != 0)
    //                parent.MoveToggle(false);
    //            return BehaviorState.Running;
    //        }
    //        var net = parent.Net;


    //        TargetArgs target = string.IsNullOrWhiteSpace(this.TargetVariableName) ? TargetArgs.Null : state.Blackboard[this.TargetVariableName] as TargetArgs;
    //        //Interaction goal = state.Blackboard.GetValueOrDefault("interaction") as Interaction;
    //        Interaction goal = parent.GetComponent<WorkComponent>().Task;
    //        if(goal == null)
    //        {
    //            goal = this.Interaction.Clone() as Interaction;
    //            //state["interaction"] = goal;
    //            (net as Server).AIHandler.AIInteract(parent, goal, target);
    //        }

    //        switch (goal.State)
    //        {
    //            case Interaction.States.Unstarted:
    //                //if (!goal.IsValid(parent, target))
    //                //{
    //                //    // arrived to perform interaction but interaction no longer valid!
    //                //    // TODO: post message to ai log?
    //                //    return BehaviorState.Fail;
    //                //}
    //                //(net as Server).AIHandler.AIInteract(parent, goal, target);
    //                return BehaviorState.Running;

    //            case Interaction.States.Running:
    //                return BehaviorState.Running;

    //            case Interaction.States.Finished:
    //                //this.Interaction = this.Interaction.Clone() as Interaction;

    //                // WARNING! the behaviordomain returns false before reaching here!
    //                //state["interaction"] = null;
    //                //state.Blackboard.Remove("interaction");
    //                return BehaviorState.Success;

    //            case Interaction.States.Failed:
    //                return BehaviorState.Fail;

    //            default:
    //                break;
    //        }
    //        return BehaviorState.Running;
    //    }
       
    //    public override object Clone()
    //    {
    //        throw new Exception();
    //        return new BehaviorInteraction(this.TargetVariableName, this.Interaction.Clone() as Interaction);
    //    }
    //}
}
