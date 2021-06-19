using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorUntilFail : Behavior
    {
        protected Behavior Child;
        public BehaviorUntilFail(Behavior child)
        {
            this.Child = child;
        }
        public BehaviorUntilFail()
        {

        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var result = this.Child.Execute(parent, state);
            return result == BehaviorState.Fail ? BehaviorState.Success : BehaviorState.Running; //return child result or running?
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
            return new BehaviorUntilFail(this.Child);
        }
    }
}
