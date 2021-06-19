using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.AI
{
    class BehaviorSelector : Behavior
    {
        public BehaviorSelector(params Behavior[] behavs)
            : base(behavs)
        { }
        Behavior Current;
        public override string ToString()
        {
            return this.Current != null ? this.Current.ToString() : "<none>";
        }
        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            
            foreach (var child in this.Children)
            {
                var result = child.Execute(parent, state);
                //if (result == BehaviorState.Success)
                // dont traverse next child if the current child is running
                if (result == BehaviorState.Success)// || result == BehaviorState.Running)
                //if (result == BehaviorState.Running)
                {
                    this.Current = child;
                    return BehaviorState.Success;
                }
            }
            //this.Current = null;
            return BehaviorState.Fail;
        }
        public override Behavior Initialize(GameObject parent)
        {
            foreach (var child in this.Children)
                child.Initialize(parent);
            return this;
        }
        public override Behavior Initialize(AIState state)
        {
            foreach (var child in this.Children)
                child.Initialize(state);
            return this;
        }
        public override object Clone()
        {
            return new BehaviorSelector((from child in this.Children select child.Clone() as Behavior).ToArray());
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    foreach (var child in this.Children)
        //        child.HandleMessage(parent, e);
        //    return false;
        //}
    }
}
