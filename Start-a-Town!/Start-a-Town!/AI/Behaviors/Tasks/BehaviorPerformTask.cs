﻿using Start_a_Town_.AI.Behaviors;
using System;
using System.Collections.Generic;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    abstract public class BehaviorPerformTask : Behavior
    {
        //public GameObject Actor;
        public AITask Task { get { 
                return this.Actor.CurrentTask;
            } set { this.Actor.CurrentTask = value; } }
        //IEnumerator<Behavior> Steps;
        protected abstract IEnumerable<Behavior> GetSteps();
        int CurrentStepIndex;
        public bool Finished;
        readonly List<Action> FinishActions = new();

        List<Behavior> _CachedBehaviors;
        List<Behavior> CachedBehaviors// = new List<Behavior>();
        {
            get
            {
                if (this._CachedBehaviors == null)
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
        //Behavior CurrentBehavior
        //{ get { return this.CachedBehaviors[this.CurrentStepIndex]; } }

        public BehaviorPerformTask()
        {

        }
        public (BehaviorState result, Behavior source) Tick(Actor parent, AIState state)
        {
            var current = this.CachedBehaviors[this.CurrentStepIndex];
            var result = this.Execute(parent, state);
            return (result, current);
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.HasFailedOrEnded())
                return BehaviorState.Fail;

            //if (this.Steps == null)
            //{
            //    this.Steps = this.GetSteps().GetEnumerator();
            //    this.Steps.MoveNext();
            //    this.CurrentStepIndex++;
            //}
            //var current = this.Steps.Current;
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
                //if (current.HasFailedOrEnded())
                //    result = BehaviorState.Fail;
                if (current.HasFailedOrEnded())
                    return BehaviorState.Fail;
                switch (result)
                {
                    case BehaviorState.Running:
                        FromJump = false;
                        //if (current.HasFailedOrEnded()) // check this before or after executing the current step?
                        //    return BehaviorState.Fail;
                        return BehaviorState.Running;

                    case BehaviorState.Success:
                        if(!FromJump) // workaround
                        {
                            //throw new Exception("jumped to a behavior but immediately skipped it in the same tick");
                            NextBehavior();
                            var hasNext = this.CachedBehaviors.Count > this.CurrentStepIndex;//  this.Steps.MoveNext();

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
            //return;
            //base.Write(w);
            //this.Task.Write(w);
            //w.Write(this.CurrentStepIndex);
            //this.Steps.Current.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            //return;

            //base.Read(r);
            //this.Task = AITask.Load(r);
            //this.CurrentStepIndex = r.ReadInt32();

            ////this.Steps.Current.Read(r);
            //if (this.Steps == null)
            //{
            //    this.Steps = this.GetSteps().GetEnumerator();
            //    this.Steps.MoveNext();
            //}
            //    for (int i = 0; i < this.CurrentStepIndex - 1; i++)
            //        this.Steps.MoveNext();
            
            //this.Steps.Current.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.CurrentStepIndex.Save("CurrentStep"));

            //tag.Add(this.CurrentBehavior.Save("Behavior"));

            //return tag;
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            //this.Task = AITask.Load(tag["Task"]);
            
            var currentStep = tag.GetValue<int>("CurrentStep");
            this.CurrentStepIndex = currentStep;
            //this.CurrentBehavior.Load(tag["Behavior"]);
        }
        //internal override void ObjectLoaded(GameObject parent)
        //{
        //    this.Steps.Current.ObjectLoaded(parent);
        //}
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
            //return this.Task.ReserveTargets(this.Actor);
        }
        protected virtual bool InitExtraReservations()
        {
            return true;
        }
        public virtual void CleanUp() 
        {
            //this.Actor.Town.ReservationManager.Unreserve(this.Actor, this.Task); // UNDONE i'm unreserving elsewhere
            for (int i = 0; i < this.FinishActions.Count; i++)
            {
                this.FinishActions[i]();
            }
            this.Actor.Net.Report(string.Format("{0} cleaned up", this));
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
            //this.CurrentStepIndex = this.CachedBehaviors.IndexOf(bhav) - 1; //because it's increased by one 
            this.CurrentStepIndex = this.CachedBehaviors.IndexOf(bhav); //because it's increased by one 
        }

        protected void AddFinishAction(Action a)
        {
            this.FinishActions.Add(a);
        }
        //public BehaviorPerformTask FailOn(Func<bool> condition)
        //{
        //    //this.AddEndCondition(condition);
        //    base.FailOn(condition);
        //    return this;
        //}
    }
}
