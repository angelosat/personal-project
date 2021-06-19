﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorSucceeder : Behavior
    {
        Behavior Child;
        public BehaviorSucceeder(Behavior child)
        {
            this.Child = child;
        }
        public BehaviorSucceeder()
        {

        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.Child == null)
                return BehaviorState.Success;
            var result = this.Child.Execute(parent, state);
            if (result == BehaviorState.Running)
                return BehaviorState.Running;
            return BehaviorState.Success;
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            if (this.Child != null)
                this.Child.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            if (this.Child != null)
                this.Child.Read(r);
        }
        public override object Clone()
        {
            return new BehaviorSucceeder(this.Child);
        }
    }
}
