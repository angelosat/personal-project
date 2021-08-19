﻿using System;
using System.Collections.Generic;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    abstract public class BehaviorPerformTask : Behavior
    {
        public AITask Task
        {
            get => this.Actor.CurrentTask;
            set => this.Actor.CurrentTask = value;
        }
        protected abstract IEnumerable<Behavior> GetSteps();
        int CurrentStepIndex;
        public bool Finished;
        readonly List<Action> FinishActions = new();

        List<Behavior> _CachedBehaviors;
        List<Behavior> CachedBehaviors
        {
            get
            {
                if (this._CachedBehaviors is null)
                {
                    this._CachedBehaviors = new List<Behavior>();
                    foreach (var bhav in this.GetSteps())
                    {
                        bhav.Actor = this.Actor;
                        this._CachedBehaviors.Add(bhav);
                    }
                }
                return this._CachedBehaviors;
            }
        }
        Behavior CurrentBehavior => this.CachedBehaviors[this.CurrentStepIndex];
        public BehaviorPerformTask()
        {

        }
        public (BehaviorState result, Behavior source) Tick(Actor parent, AIState state)
        {
            var current = this.CachedBehaviors[this.CurrentStepIndex];
            var result = this.Execute(parent, state);
            return (result, current);
        }
        public sealed override BehaviorState Execute(Actor parent, AIState state)
        {
            if(this.Task.IsCancelled)
                return BehaviorState.Fail;
            if (this.HasFailedOrEnded())
                return BehaviorState.Fail;
            var current = this.CachedBehaviors[this.CurrentStepIndex];
            if (current != null)
            {
                // MOVING THIS TO BEHAVIOR'S EXECUTE FUNCTION
                // MOVING THIS AFTER BEHAVIOR'S EXECUTE FUNCTION because the behavior might update values that will make it not fail, and this should happen before the fail check for this tick
                // why did i actually do this again???
                //var failedorended = current.HasFailedOrEnded();
                //if (failedorended)
                //    parent.Net.Log.Write(current.ToString() + " failed or ended");

                current.PreTick();
                if (current != this.CachedBehaviors[this.CurrentStepIndex]) // if the pretick action caused a jump, return
                    return BehaviorState.Running;
                FromJump = false;

                // IF I CALL THIS HERE
                // when an actor adds an item to his existing carried item stack, the target item gets absorbed to the carried stack and stops existing
                // since the target item no longer exists, calling this here for some reason fails the 'target existing' check
                // WORKCOMPONENT is ticked after AICOMPONENT, so the interaction finishes and changes the game state before the behavior that handles the interaction is called
                // the behavior that handles the interaction doesn't get the chance to return success and advance the parent behavior
                //if (current.HasFailedOrEnded())
                //    return BehaviorState.Fail;
                //var result = failedorended ? BehaviorState.Fail : current.Execute(parent, state);

                var result = current.Execute(parent, state);
                this.Task.TicksTotal++;
                /// added the success check because interactioncrafting in behaviorcrafting fails even after the interaction successfuly completes because the ingredients are disposed, and it fails on disposed ingredients
                /// move the whole if block inside the switch block below?
                //if (result != BehaviorState.Success && current.HasFailedOrEnded())
                //    return BehaviorState.Fail;

                switch (result)
                {
                    case BehaviorState.Running:
                        FromJump = false;
                        if (current.HasFailedOrEnded())   /// have this here or before the switch block?
                            return BehaviorState.Fail;
                        return BehaviorState.Running;

                    case BehaviorState.Success:
                        if(!FromJump) // workaround
                        {
                            NextBehavior();
                            var hasNext = this.CachedBehaviors.Count > this.CurrentStepIndex;

                            if (!hasNext)
                                return BehaviorState.Success;
                        }

                        this.CachedBehaviors[this.CurrentStepIndex].PreInitAction();
                        this.FromJump = false;
                        return BehaviorState.Running;

                    case BehaviorState.Fail:
                        FromJump = false;
                        return BehaviorState.Fail;
                }
            }
            FromJump = false;
            return BehaviorState.Success;
        }

        private void NextBehavior()
        {
            this.CurrentStepIndex++;
        }

        
        public override void Write(System.IO.BinaryWriter w)
        {
        }
        public override void Read(System.IO.BinaryReader r)
        {
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.CurrentStepIndex.Save("CurrentStep"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            var currentStep = tag.GetValue<int>("CurrentStep");
            this.CurrentStepIndex = currentStep;
        }
       
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        public bool InitBaseReservations()
        {
            if (this.Task.Tool.HasObject)
                if (!this.Actor.Reserve(this.Task.Tool, 1))
                    return false;

            return this.InitExtraReservations();
        }
        protected virtual bool InitExtraReservations()
        {
            return true;
        }
        public virtual void CleanUp() 
        {
            for (int i = 0; i < this.FinishActions.Count; i++)
                this.FinishActions[i]();
            this.Actor.Net.Report($"{this} cleaned up");
        }
        internal override void MapLoaded(Actor parent)
        {
            this.Actor = parent;
            this.Task.MapLoaded(parent);
        }
        bool FromJump = false;
        public void JumpTo(Behavior bhav)
        {
            FromJump = true;
            this.CurrentStepIndex = this.CachedBehaviors.IndexOf(bhav); //because it's increased by one 
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            this.CurrentBehavior.ObjectLoaded(parent);
        }
        protected void AddFinishAction(Action a)
        {
            this.FinishActions.Add(a);
        }
    }
}
