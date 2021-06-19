using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.AI;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    public enum BehaviorState { Running, Success, Fail }
    public abstract class Behavior : ICloneable
    {
        //public string Name = "";
        public virtual string Name { get { return ""; } }
        public string Label;
        /// <summary>
        /// This action is performed before any end conditions are checked
        /// </summary>
        public Action PreInitAction = () => { };

        readonly List<Func<BehaviorState>> EndConditions = new();
        readonly List<Action> PreTickActions = new();

        public void PreTick()
        {
            for (int i = 0; i < this.PreTickActions.Count; i++)
            {
                this.PreTickActions[i]();
            }
        }

        public void AddEndCondition(Func<BehaviorState> cond)
        {
            this.EndConditions.Add(cond);
        }
        public void AddPreTickAction(Action act)
        {
            this.PreTickActions.Add(act);
        }
        //public virtual bool HasEnded()
        //{
        //    foreach (var cond in this.EndConditions)
        //        if (cond() == BehaviorState.Fail)
        //            return true;
        //    return false;
        //}
        public virtual bool HasFailedOrEnded()
        {
            if (!this.EndConditions.Any())
                return false;
            //foreach (var cond in this.EndConditions)
            //    if (cond() == BehaviorState.Running)
            //        return false;
            foreach (var cond in this.EndConditions)
            {
                var result = cond();
                if (result == BehaviorState.Success || result == BehaviorState.Fail)
                    return true;
            }
            return false;
        }
        public Behavior FailOn(Func<bool> cond)
        {
            //this.AddEndCondition(cond);
            this.AddEndCondition(() =>
            {
                if (cond())
                    return BehaviorState.Fail;
                return BehaviorState.Running;
            });
            return this;
        }
        public Behavior JumpIf(Func<bool> cond, Behavior gotoBhav)
        {
            this.AddPreTickAction(() =>
            {
                if (cond())
                    this.Actor.CurrentTaskBehavior.JumpTo(gotoBhav);
            });
            return this;
        }
        public Actor Actor;

        public abstract BehaviorState Execute(Actor parent, AIState state);// { throw new Exception(); }

        public virtual void Write(BinaryWriter w)
        {
            
        }
        public virtual void Read(BinaryReader r)
        {
            
        }

        public Behavior Not()
        {
            return new BehaviorInverter(this);
        }
        public abstract object Clone();
        public BehaviorDomain While(Func<GameObject, AIState, bool> condition)
        {
            return new BehaviorDomain(new BehaviorCondition(condition), this);
        }
        public BehaviorDomain While(Func<bool> condition)
        {
            return new BehaviorDomain(new BehaviorCondition((a, t) => condition()), this);
        }

        internal SaveTag Save(string name = "")
        {
            //throw new NotImplementedException();
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.String, "Type", this.GetType().FullName));
            this.AddSaveData(tag);
            return tag;
        }
        protected virtual void AddSaveData(SaveTag tag) { }
        internal virtual void Load(SaveTag tag)
        {
            //throw new NotImplementedException();
        }

        internal virtual void ObjectLoaded(GameObject parent)
        {
            
        }
        internal virtual void WriteBlackboard(BinaryWriter w, Dictionary<string, object> blackboard) { }
        internal virtual void ReadBlackboard(BinaryReader r, Dictionary<string, object> blackboard) { }
        internal virtual SaveTag SaveBlackboard(string name, Dictionary<string, object> blackboard) { return null; }
        internal virtual void LoadBlackboard(SaveTag tag, Dictionary<string, object> blackboard) { }

        internal virtual void MapLoaded(Actor parent)
        {
            
        }
    }

    
}

