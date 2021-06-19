﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorRepeat : Behavior
    {
        Behavior Child;
        BehaviorCondition Condition;
        public BehaviorRepeat(BehaviorCondition condition, Behavior child)
            : this(child, condition)
        {

        }

        public BehaviorRepeat(Behavior child, BehaviorCondition condition)
        {
            this.Child = child;
            this.Condition = condition;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            this.Child.Execute(parent, state);
            var eval = this.Condition.Evaluate(parent, state);
            if (eval)
                return BehaviorState.Success;
            else
                return BehaviorState.Running;
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            this.Child.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Child.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorRepeat(this.Child.Clone() as Behavior, this.Condition);
        }
    }
}
