using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;

namespace Start_a_Town_.AI.Behaviors
{
    public class BehaviorDomain : BehaviorDecorator
    {
        //Behavior Child;
        BehaviorCondition Condition;
        public BehaviorDomain(BehaviorCondition condition, Behavior child)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public BehaviorDomain(Behavior child, BehaviorCondition condition)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //if (!this.Condition.Evaluate(parent, state))
            //    return BehaviorState.Fail;
            //else
            //    return this.Child.Execute(parent, state);

            var cond = this.Condition.Execute(parent, state);
            if (cond == BehaviorState.Fail)
                return BehaviorState.Fail;
            else
                return this.Child.Execute(parent, state);

        }
        //public override void Write(System.IO.BinaryWriter w)
        //{
        //    this.Child.Write(w);
        //}
        //public override void Read(System.IO.BinaryReader r)
        //{
        //    this.Child.Read(r);
        //}
        public override object Clone()
        {
            //return new BehaviorDomain(this.Child.Clone() as Behavior, this.Condition);
            return new BehaviorDomain(this.Condition, this.Child.Clone() as Behavior);
        }
    }
}
